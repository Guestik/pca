using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using System.IO;
using System.Text.RegularExpressions;

namespace pca
{
    class Program
    {
        static void Main(string[] args)
        {
            List<double[]> dataX = new List<double[]>(); //All data of all classes together
            List<string> classes = new List<string>(); //List of classes
            Regex dotPattern = new Regex("[.]"); //Need to replace ',' because otherwise can not parse numbers
            StreamReader sr = new StreamReader("training_data_classification.txt"); //Input file
            string line = sr.ReadLine();
            while (line != null) //While there is something to read from the file
            {
                string[] split = line.Split(',');

                for (int tempik = 0; tempik < split.Length; tempik++)
                    split[tempik] = dotPattern.Replace(split[tempik], ",");

                double[] itemik = new double[split.Length - 1]; //Minus 1 because the last one is a class
                for (int i = 0; i < split.Count() - 1; i++)
                    itemik[i] = double.Parse(split[i]);

                classes.Add(split[split.Count() - 1]); //Last one is class, load it to our list

                dataX.Add(itemik);
                line = sr.ReadLine(); //Read next line
            }
            sr.Close(); //Close the file

            int numOfDimensions = dataX[0].Length; //Number of input dimensions
            int newDimensions = 2; //Number of dimensions to be reduced to
            int numberOfData = dataX.Count(); //Number of all data

            //Calculate mean of all data
            double[] mi = Mi(dataX, numOfDimensions);

            //Calculate variance of all data
            double[] variance = Variance(dataX, numOfDimensions, mi);

            //Calculate covariance of all data
            double[,] coverianceMatrix = Covariance(dataX, numOfDimensions, mi, variance);

            //Get sorted eigenvelues and eigenvectors
            Accord.Math.Decompositions.EigenvalueDecomposition dve = new Accord.Math.Decompositions.EigenvalueDecomposition(coverianceMatrix, false, true);

            //Calculate projection matrix using 2 eigenvectors associated with 2 largest eigenvalues
            double[,] W = new double[dve.Eigenvectors.GetLength(1), newDimensions];
            for (int i = 0; i < dve.Eigenvectors.GetLength(1); i++)
            {
                for (int q = 0; q < newDimensions; q++)
                {
                    W[i, q] = dve.Eigenvectors[i, q];
                }
            }

            //Calculate z
            List<double[]> z = new List<double[]>();
            for (int i = 0; i < numberOfData; i++)
                z.Add(Matrix.Dot(Matrix.Transpose(W), Elementwise.Subtract(dataX[i], mi)));

            //Export reduced dataset with corresponding classes to the file
            FileStream stream = new FileStream("answer_data_pca.txt", FileMode.Create);
            StreamWriter file = new StreamWriter(stream);
            using (file)
            {
                for (int i = 0; i < z.Count(); i++)
                    file.WriteLine("{0};{1};{2}", z[i][0], z[i][1], classes[i]);
            }
        }

        public static double[] Mi(List<double[]> dataX, int pocetDimenzi)
        {
            double[] mi = new double[pocetDimenzi];
            for (int y = 0; y < dataX.Count; y++) //For every N
                for (int x = 0; x < dataX[y].Length; x++) //For every element in transaction
                    mi[x] += dataX[y][x];

            for (int d = 0; d < pocetDimenzi; d++)
            {
                mi[d] = mi[d] / dataX.Count;
            }
            return mi;
        }

        public static double[] Variance(List<double[]> dataX, int dimensions, double[] mi)
        {
            double[] varOfEachColumn = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                for (int j = 0; j < dataX.Count; j++)
                {
                    varOfEachColumn[i] += Math.Pow(dataX[j][i] - mi[i], 2);
                }
            }
            for (int d = 0; d < varOfEachColumn.Length; d++)
            {
                varOfEachColumn[d] = varOfEachColumn[d] / dataX.Count;
            }
            return varOfEachColumn;
        }

        public static double Coveriance(double[] dimenzeA, double meanA, double[] dimenzeB, double meanB, int numberOfData)
        {
            double result = 0;
            for (int a = 0; a < dimenzeA.Length; a++) //Both dimensions are same length, so it doesnt matter which I put here
                result += (dimenzeA[a] - meanA) * (dimenzeB[a] - meanB);
            return result / numberOfData;
        }

        public static double[,] Covariance(List<double[]> dataX, int pocetDimenzi, double[] mi, double[] varKazdehoSloupce)
        {
            //Covariance (d*d matrix)
            double[,] coverianceMatrix = new double[pocetDimenzi, pocetDimenzi];
            double[] dimenzeA_temp = new double[dataX.Count];
            double[] dimenzeB_temp = new double[dataX.Count];

            for (int x = 0; x < pocetDimenzi; x++)
            {
                for (int y = 0; y < pocetDimenzi; y++)
                {
                    if (x == y) //Variance on the diagonal
                    {
                        coverianceMatrix[x, y] = varKazdehoSloupce[x];
                    }
                    else
                    {
                        for (int i = 0; i < dataX.Count; i++)
                        {
                            dimenzeA_temp[i] = dataX[i][x];
                            dimenzeB_temp[i] = dataX[i][y];
                        }

                        coverianceMatrix[x, y] = Coveriance(dimenzeA_temp, mi[x], dimenzeB_temp, mi[y], dataX.Count);
                    }
                }
            }
            return coverianceMatrix;
        }
    }
}
