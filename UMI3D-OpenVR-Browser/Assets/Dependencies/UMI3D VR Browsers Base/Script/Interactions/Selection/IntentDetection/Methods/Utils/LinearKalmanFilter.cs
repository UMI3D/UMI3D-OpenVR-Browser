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

namespace utils.KalmanFilter
{
    /// <summary>
    /// To use with linear models only
    /// Row majors matrices are expected
    /// </summary>
    public class LinearKalmanFilter : AbstractKalmanFilter
    {
        /// <summary>
        /// State-transition model, also known as dynamic model
        /// </summary>
        private Matrix<double> Fk;

        /// <summary>
        /// Observation model
        /// </summary>
        private Matrix<double> Hk;

        public LinearKalmanFilter(double stdProcess, double stdMeasure, double[][] modelProcess, double[][] modelObservation)
            : base(modelProcess.Length, modelObservation.Length)
        {
            q = stdProcess;
            r = stdMeasure;

            Q = Matrix.Build.Diagonal(L, L, q * q);
            R = Matrix.Build.Diagonal(M, M, r * r);

            Fk = Matrix.Build.DenseOfRowArrays(modelProcess);
            Hk = Matrix.Build.DenseOfRowArrays(modelObservation);
        }

        public LinearKalmanFilter(double stdProcess, double stdMeasure, Matrix<double> modelProcess, Matrix<double> modelObservation)
            : base(modelProcess.RowCount, modelObservation.RowCount)
        {
            q = stdProcess;
            r = stdMeasure;

            Q = Matrix.Build.Diagonal(L, L, q * q);
            R = Matrix.Build.Diagonal(M, M, r * r);

            Fk = modelProcess;
            Hk = modelObservation;
        }

        /// <summary>
        /// Produce a Kalman filter with observation of the same dimension than the states and with an identity observation model
        /// </summary>
        /// <param name="stdProcess"></param>
        /// <param name="stdMeasure"></param>
        /// <param name="modelProcess"></param>
        public LinearKalmanFilter(double stdProcess, double stdMeasure, double[][] modelProcess)
            : this(stdProcess, stdMeasure, modelProcess, Matrix.Build.DenseIdentity(modelProcess.Length, modelProcess.Length).ToRowArrays())
        { }

        /// <summary>
        /// Produce a Kalman filter with observation of the same dimension than the states and with an identity observation model
        /// </summary>
        /// <param name="stdProcess"></param>
        /// <param name="stdMeasure"></param>
        /// <param name="modelProcess"></param>
        public LinearKalmanFilter(double stdProcess, double stdMeasure, Matrix<double> modelProcess)
            : this(stdProcess, stdMeasure, modelProcess, Matrix.Build.DenseIdentity(modelProcess.RowCount, modelProcess.ColumnCount))
        { }

        #region Init

        public override void Init(double[] estimateGuessed, double[][] covarianceInit)
        {
            x_est = Vector.Build.Dense(estimateGuessed);
            P = Matrix.Build.DenseOfRowArrays(covarianceInit);

            filterStepState = FilterState.Initialized;
        }

        #endregion Init

        #region Predict

        public override double[] Predict()
        {
            if (!(filterStepState == FilterState.JustUpdated || filterStepState == FilterState.Initialized))
                throw new Exception("Trying to predict without update or initialization");

            x_est_predicted = Fk * x_est;
            P_predicted = Fk * P * Fk.Transpose() + Q; //prediction from the model

            filterStepState = FilterState.JustPredicted;
            return x_est_predicted.ToArray();
        }

        public double[] Predict(double[][] modelProcess)
        {
            Fk = Matrix.Build.DenseOfRowArrays(modelProcess);
            return Predict();
        }

        public double[] Predict(Matrix<double> modelProcess)
        {
            Fk = modelProcess;
            return Predict();
        }

        #endregion Predict

        #region Update

        public override double[] Update(double[] measure)
        {
            if (filterStepState != FilterState.JustPredicted)
                throw new Exception("Trying to update without a prediction");

            var S = Hk * P_predicted * Hk.Transpose() + R;
            var K = P_predicted * Hk.Transpose() * S.Inverse(); // Kalman optimal gain

            var Z = Vector.Build.Dense(measure);
            var deviation = Z - Hk * x_est_predicted;

            x_est = x_est_predicted + K * deviation; //estimation update

            filterStepState = FilterState.JustUpdated;
            return x_est.ToArray();
        }

        public double[] Update(double[] measure, double[][] modelObservation)
        {
            Hk = Matrix.Build.DenseOfRowArrays(modelObservation);
            return Update(measure);
        }

        #endregion Update
    }
}