/*
Copyright 2019 - 2021 Inetum
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;

namespace utils.KalmanFilter
{
    /// <summary>
    /// UKF as described in Wan & van der Merwe 2000
    /// </summary>
    public class UnscentedKalmanFilter : AbstractKalmanFilter
    {
        /// <summary>
        /// The alpha coefficient, characterize sigma-points dispersion around mean
        /// </summary>
        private double alpha;

        /// <summary>
        /// Kappa factor, secondary parameter usually set to 0
        /// </summary>
        private double kappa;

        /// <summary>
        /// The beta coefficient, characterize type of distribution (2 for normal one)
        /// </summary>
        private double beta;

        /// <summary>
        /// Scale factor
        /// </summary>
        private double lambda;

        /// <summary>
        /// Mean weights
        /// </summary>
        private Vector<double> Wm;

        /// <summary>
        /// Covariance weights
        /// </summary>
        private Vector<double> Wc;

        /// <summary>
        /// Non-linear map of the process model
        /// </summary>
        private Func<Vector<double>, Vector<double>> f;

        /// <summary>
        /// Non-linear map of the observation model
        /// </summary>
        private Func<Vector<double>, Vector<double>> h;

        /// <summary>
        /// Unscented transform of the process model
        /// </summary>
        private UnscentedTransformResults ut_F;

        private class UnscentedTransformResults
        {
            /// <summary>
            /// propagated mean, y
            /// </summary>
            public Vector<double> mean;

            /// <summary>
            /// propagated sigma vectors, Y
            /// </summary>
            public List<Vector<double>> samplingPoints;

            /// <summary>
            /// covariance matrix, P
            /// </summary>
            public Matrix<double> covariance;

            /// <summary>
            /// deviation around mean, (Y_k-y)_k
            /// </summary>
            public Matrix<double> deviations;
        }

        /// <summary>
        /// Constructor of Unscented Kalman Filter
        /// </summary>
        /// <param name="L">State dimension</param>
        /// <param name="M">Measurements dimension</param>
        /// <param name="q">std of process</param>
        /// <param name="r">std of measurement</param>
        public UnscentedKalmanFilter(int L, int M, double q, double r, Func<double[], double[]> processModel, Func<double[], double[]> observationModel)
            : base(L, M)
        {
            this.q = q;
            this.r = r;

            this.f = x => Vector.Build.Dense(processModel(x.ToArray()));
            this.h = x => Vector.Build.Dense(observationModel(x.ToArray()));
        }

        #region Init

        public override void Init(double[] initialStateGuessed, double[][] initialCovariance)
        {
            Q = Matrix.Build.Diagonal(L, L, q * q); //covariance of process
            R = Matrix.Build.Dense(M, M, r * r); //covariance of measurement

            // unscented transform parameters
            alpha = 1e-3d;
            kappa = 0;
            beta = 2d;
            lambda = alpha * alpha * (L + kappa) - L;
            var c = L + lambda;

            //weights for means
            Wm = Vector.Build.Dense(2 * L + 1, 0.5 / c);
            Wm[0] = lambda / c;

            //weights for covariance
            Wc = Vector.Build.Dense(2 * L + 1);
            Wm.CopyTo(Wc);
            Wc[0] = Wm[0] + 1 - alpha * alpha + beta;

            filterStepState = FilterState.Initialized;
        }

        #endregion Init

        #region Predict

        public override double[] Predict()
        {
            if (!(filterStepState == FilterState.JustUpdated || filterStepState == FilterState.Initialized))
                throw new Exception("Trying to predict without update or initialization");

            //sigma points around x
            List<Vector<double>> X = GetSigmaPoints(x_est, P);

            //unscented transformation of process
            ut_F = UnscentedTransform(f, X, Wm, Wc, L, Q);

            //a priori estimated state
            x_est_predicted = ut_F.mean;

            filterStepState = FilterState.JustPredicted;
            return x_est_predicted.ToArray();
        }

        public double[] Predict(Func<Vector<double>, Vector<double>> processModel)
        {
            f = processModel;
            return Predict();
        }

        public double[] Predict(Func<double[], double[]> processModel)
        {
            f = x => Vector.Build.DenseOfArray(processModel(x.ToArray()));
            return Predict();
        }

        #endregion Predict

        #region Update

        public override double[] Update(double[] measurements)
        {
            if (filterStepState != FilterState.JustPredicted)
                throw new Exception("Trying to update without prediction");

            // integating measures
            var z = Vector.Build.Dense(measurements);

            //unscented transformation of sampling points
            UnscentedTransformResults ut_H = UnscentedTransform(h, ut_F.samplingPoints, Wm, Wc, M, R);

            //a priori estimated output
            var z_est_apriori = ut_H.mean;

            //transformed cross-covariance
            Matrix<double> Pxy = ut_F.deviations * Matrix.Build.DiagonalOfDiagonalVector(Wc) * ut_H.deviations.Transpose();

            // Kalman gain matrix
            Matrix<double> K = Pxy * ut_H.covariance.Inverse();

            //state update a posteriori
            x_est = x_est_predicted + K * (z - z_est_apriori);

            //covariance update a posteriori
            P = ut_F.covariance - K * Pxy.Transpose();

            filterStepState = FilterState.JustUpdated;
            return x_est.ToArray();
        }

        public double[] Update(double[] measure, Func<double[], double[]> observationModel)
        {
            h = x => Vector.Build.DenseOfArray(observationModel(x.ToArray()));
            return Update(measure);
        }

        public double[] Update(double[] measure, Func<Vector<double>, Vector<double>> observationModel)
        {
            h = observationModel;
            return Update(measure);
        }

        #endregion Update

        #region UnscentedTransform

        /// <summary>
        /// Unscented Transformation
        /// </summary>
        /// <param name="f">nonlinear map</param>
        /// <param name="X">sigma vectors</param>
        /// <param name="Wm">Weights for means</param>
        /// <param name="Wc">Weights for covariance</param>
        /// <param name="n">numer of outputs of f</param>
        /// <param name="R">additive covariance</param>
        /// <returns>[transformed mean, transformed sampling points, transformed covariance, transformed deviations]</returns>
        private UnscentedTransformResults UnscentedTransform(Func<Vector<double>, Vector<double>> f, List<Vector<double>> X, Vector<double> Wm, Vector<double> Wc, int n, Matrix<double> R)
        {
            int numberOfSigmaPoints = X.Count;
            Vector<double> y = Vector.Build.Dense(n);
            List<Vector<double>> Y = new List<Vector<double>>();
            Matrix<double> Y1 = Matrix.Build.Dense(L, numberOfSigmaPoints, 0);

            Vector<double> Xk;
            for (int k = 0; k < numberOfSigmaPoints; k++)
            {
                Xk = X[k];
                var Yk = f(Xk);
                Y.Add(Yk);
                y += Yk * Wm[k];
                Y1.SetColumn(k, Yk - y);
            }

            Matrix<double> P = Y1 * Matrix.Build.DiagonalOfDiagonalVector(Wc) * Y1.Transpose() + R;

            var output = new UnscentedTransformResults()
            {
                mean = y,
                samplingPoints = Y,
                covariance = P,
                deviations = Y1
            };
            return output;
        }

        /// <summary>
        /// Sigma points around reference point
        /// </summary>
        /// <param name="x">reference point</param>
        /// <param name="P">covariance</param>
        /// <param name="c">coefficient sqrt(lambda+L)</param>
        /// <returns>Sigma points</returns>
        private List<Vector<double>> GetSigmaPoints(Vector<double> x, Matrix<double> P)
        {
            int L = x.Count;
            var scaleFactor = Math.Sqrt(L + lambda);

            Matrix<double> sqrtP = P.Cholesky().Factor;

            var A = sqrtP.Transpose() * scaleFactor;

            Matrix<double> Y = Matrix.Build.Dense(L, L, 1);
            for (int j = 0; j < L; j++)
            {
                Y.SetSubMatrix(0, L, j, 1, x.ToColumnMatrix());
            }

            var sigmaPoints = new List<Vector<double>> { x };

            var YplusA = Y + A;
            for (int k = 1; k < L + 1; k++)
            {
                sigmaPoints.Add(YplusA.Column(k));
            }

            var YminusA = Y - A;
            for (int k = L + 1; k < (2 * L + 1); k++)
            {
                sigmaPoints.Add(YminusA.Column(k));
            }

            return sigmaPoints;
        }

        #endregion UnscentedTransform
    }
}