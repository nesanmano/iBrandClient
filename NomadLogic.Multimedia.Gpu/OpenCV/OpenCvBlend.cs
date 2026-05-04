using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using NLog;

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// Cette classe regroupe des méthodes pour manipuler des matrices de valeurs RGBA normalisée entre [0..1], afin d'effectuer des opérations
    /// optimisées pour faire du blending de pixel via OpenCv.
    /// TOUTES les méthode décrites ici fonctionnent sur le fait que les couleurs sont en format BGR, et que toutes ces valeurs sont normalisées
    /// dans un float, donc avec le range de valeur [0..1]
    /// Voir les liens suivants pour plus d'information sur les résultats attendus et les algorithmes.:
    /// https://stackoverflow.com/questions/5919663/how-does-photoshop-blend-two-images-together
    /// https://en.wikipedia.org/wiki/Blend_modes
    /// http://www.rw-designer.com/blending-mode
    /// https://support.allegorithmic.com/documentation/display/SDDOC/Blending+modes+description
    public static class OpenCvBlend
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const DepthType DPT = DepthType.Cv32F;

        private static void BlendMatrixPremultipliedColorValues(Mat bgrA, Mat bgrB, Mat alphaA, Mat alphaB, out Mat bgraResult,
            Action<Mat, Mat, Mat, Mat> calculateFinalAlphaMethod, 
            Action<Mat, Mat, Mat, Mat, Mat, Mat> calculateTargetBgrMatrix)
        {
            var rows = bgrA.Rows;
            var cols = bgrA.Cols;
            
            using (
                Mat bR =         new Mat(rows, cols, DPT, 1),
                gR =             new Mat(rows, cols, DPT, 1),
                rR =             new Mat(rows, cols, DPT, 1),
                aR =             new Mat(rows, cols, DPT, 1),
                invertAlphaA =   new Mat(rows, cols, DPT, 1))
            {
                var matTab = new[]
                {
                    bR, gR, rR
                };
                var splitA = bgrA.Split();
                var splitB = bgrB.Split();

                // Invertion de l'alpha de A
                alphaA.ConvertTo(invertAlphaA, DepthType.Cv32F, -1.0, 1.0); // Équivalent de (1 - A), soit ((-A) + 1)

                if (log.IsTraceEnabled)
                {
                    log.Trace("Mats:\n\tinvertAlpha= {0}\n\talphaA=\t\t\t{1}\n\talphaB=\t\t\t{2}\n\ttmp=\t\t\t{3}", 
                        OpenCvUtil.GetMatHeader(invertAlphaA),
                        OpenCvUtil.GetMatHeader(alphaA), 
                        OpenCvUtil.GetMatHeader(alphaB));
                }

                for (var i = 0; i < 3; i++)
                {
                    if (log.IsTraceEnabled)
                    {
                        log.Trace("\tsplitA[{0}]=\t\t\t{1}\n\tsplitB[{0}]=\t\t\t{2}\n\tmatTab[{0}]=\t\t\t{3}",
                            i,
                            OpenCvUtil.GetMatHeader(splitA[i]),
                            OpenCvUtil.GetMatHeader(splitB[i]),
                            OpenCvUtil.GetMatHeader(matTab[i]));
                    }
                    
                    // Calculs matriciels de Blending
                    calculateTargetBgrMatrix.Invoke(splitA[i], splitB[i], alphaA, alphaB, invertAlphaA, matTab[i]);
                }

                // Calcul de l'alpha résultat
                calculateFinalAlphaMethod.Invoke(alphaA, alphaB, invertAlphaA, aR);
                
                bgraResult = new Mat(rows, cols, DPT, 4);
                //Création de la matrice couleur finale
                using (var resultChannelsBgr = new VectorOfMat())
                {
                    resultChannelsBgr.Push(bR); // B
                    resultChannelsBgr.Push(gR); // G
                    resultChannelsBgr.Push(rR); // R
                    resultChannelsBgr.Push(aR); // A
                    CvInvoke.Merge(resultChannelsBgr, bgraResult);
                }
            }
        }
        
        private static readonly Action<Mat, Mat, Mat, Mat> ALPHA_PRECALC_A_B = (aA, aB, invertA, result) =>
        {
            // Calcul de l'alpha en combinant des silhouettes.
            CvInvoke.Multiply(aB, invertA, result);
            CvInvoke.Accumulate(aA, result);
        };
        
        private static readonly Action<Mat, Mat, Mat, Mat> ALPHA_PRECALC_B = (aA, aB, invertA, result) =>
        {
            // Calcul de l'alpha résultat (Juste B)
            CvInvoke.Accumulate(aB, result);
        };
        
        /// <summary>
        /// Exécute un mélange de canaux basé sur l'algorithme de "Normal"
        /// voir: https://en.wikipedia.org/wiki/Alpha_compositing
        /// </summary>
        /// <param name="bgrA">Matrice des canaux couleurs A</param>
        /// <param name="alphaA">Matrice de l'alpha de A</param>
        /// <param name="bgrB">Matrice des canaux couleurs B</param>
        /// <param name="alphaB">Matrice de l'alpha de B</param>
        /// <param name="bgraResult">Matrice complète BGRA du résultat.</param>
        public static void Normal(Mat bgrA, Mat alphaA, Mat bgrB, Mat alphaB, out Mat bgraResult)
        {
            // ((uint8)(A))
            
            var bgrAction = new Action<Mat, Mat, Mat, Mat, Mat, Mat>((cA, cB, aA, aB, invertA, result) =>
            {
                using (var tmp = new Mat(aA.Rows, aA.Cols, DPT, 1))
                {
                    // Multiplie B par son alpha, puis par inverse alpha A, puis stocke dans resultat
                    CvInvoke.Multiply(cB, aB, tmp);
                    CvInvoke.Multiply(tmp, invertA, result);

                    // Garde A tel quel, devient T

                    // Multiplie T par alpha A, puis accumule dans resultat
                    CvInvoke.Multiply(cA, aA, tmp);
                    CvInvoke.Accumulate(tmp, result);
                }
            });
            
            BlendMatrixPremultipliedColorValues(bgrA, bgrB, alphaA, alphaB, out var final, ALPHA_PRECALC_A_B, bgrAction);
            bgraResult = final;
        }
        
        /// <summary>
        /// Exécute un mélange de canaux basé sur l'algorithme de "Multiply".
        /// L'alpha final est B.
        /// </summary>
        /// <param name="bgrA">Matrice des canaux couleurs A</param>
        /// <param name="alphaA">Matrice de l'alpha de A</param>
        /// <param name="bgrB">Matrice des canaux couleurs B</param>
        /// <param name="alphaB">Matrice de l'alpha de B</param>
        /// <param name="bgraResult">Matrice complète BGRA du résultat.</param>
        public static void Multiply(Mat bgrA, Mat alphaA, Mat bgrB, Mat alphaB, out Mat bgraResult)
        {
            var bgrAction = new Action<Mat, Mat, Mat, Mat, Mat, Mat>((cA, cB, aA, aB, invertA, result) =>
            {
                // ((uint8)((A * B) / 255))
                
                using (
                    Mat tmp = new Mat(aA.Rows, aA.Cols, DPT, 1),
                    tmp2 = new Mat())
                {
                    // Multiplie B par son alpha, puis par inverse alpha A, puis stocke dans resultat
                    CvInvoke.Multiply(cB, aB, tmp);
                    CvInvoke.Multiply(tmp, invertA, result);
                    
                    // Multiplie A par B, donne T
                    CvInvoke.Multiply(cB, cA, tmp);
                    
                    // Multiplie T par alpha A, puis accumule dans resultat
                    CvInvoke.Multiply(tmp, aA, tmp2);
                    CvInvoke.Accumulate(tmp2, result);
                }
            });
            
            BlendMatrixPremultipliedColorValues(bgrA, bgrB, alphaA, alphaB, out var final, ALPHA_PRECALC_B, bgrAction);
            bgraResult = final;
        }
        
        /// <summary>
        /// Exécute un mélange de canaux basé sur l'algorithme de "Overlay".
        /// L'alpha final est B.
        /// </summary>
        /// <param name="bgrA">Matrice des canaux couleurs A</param>
        /// <param name="alphaA">Matrice de l'alpha de A</param>
        /// <param name="bgrB">Matrice des canaux couleurs B</param>
        /// <param name="alphaB">Matrice de l'alpha de B</param>
        /// <param name="bgraResult">Matrice complète BGRA du résultat.</param>
        public static void Overlay(Mat bgrA, Mat alphaA, Mat bgrB, Mat alphaB, out Mat bgraResult)
        {
            var bgrAction = new Action<Mat, Mat, Mat, Mat, Mat, Mat>((cA, cB, aA, aB, invertA, result) =>
            {
                // ((uint8)((B < 128) ? (2 * A * B / 255):(255 - 2 * (255 - A) * (255 - B) / 255)))
                // ((B < 0.5) ? (2 * A * B) : (1 - 2 * (1 - A) * (1 - B))
                using (
                    Mat tmp = new Mat(aA.Rows, aA.Cols, DPT, 1),
                    tmp2 = new Mat(aA.Rows, aA.Cols, DPT, 1),
                    tmpResult = new Mat(aA.Rows, aA.Cols, DPT, 1))
                {
                    // Pré-multiplication alpha
                    CvInvoke.Multiply(cB, aB, tmp);
                    CvInvoke.Multiply(cA, aA, tmp2);
                    CvInvoke.Multiply(tmp, invertA, result);
                    
                    // Calcul de T
                    using (Mat
                        maskOver = new Mat(aA.Rows, aA.Cols, DPT, 1),
                        maskUnder = new Mat(aA.Rows, aA.Cols, DPT, 1)
                    )
                    {
                        var mtx1 = new Matrix<float>(aA.Rows, aA.Cols);
                        var mtx2 = new Matrix<float>(aA.Rows, aA.Cols);

                        CvInvoke.Threshold(cB, maskOver, 0.5, 1, ThresholdType.Binary);
                        CvInvoke.Threshold(cB, maskUnder, 0.5, 1, ThresholdType.BinaryInv);

//                        // Portion un: B > 0.5 -> (1 - 2 * (1 - A) * (1 - B))
//                        CvInvoke.Multiply(cA, maskOver, mtx1);
//                        mtx1 = 1 - mtx1; // Donne (1 - A)
//                        CvInvoke.Multiply(tmp, maskOver, mtx2);
//                        mtx2 = 1 - mtx2; // Donne (1 - B)
//                        CvInvoke.Multiply(mtx1, mtx2, mtx1, 2); // Donne 2 * (iA) * (iB) => 2iAiB
//                        mtx2 = 1 - mtx1; // 1 - 2iAiB -> vers résultat.
//                        mtx2.Mat.CopyTo(tmpResult);

//                        // Portion 2: B <= 0.5 -> (2 * A * B)
//                        CvInvoke.Multiply(cA, maskUnder, mtx1);
//                        CvInvoke.Multiply(cB, maskUnder, mtx2);
//                        CvInvoke.Multiply(mtx1, mtx2, mtx2, 2); // Donne 2AB
//
//                        // Fusionne les deux matrices (Under, Over). Comme chacune possède pour le même indice, soit zéro, soit une valeur, on ne fait qu'additionner.
//                        CvInvoke.Accumulate(mtx2, tmpResult);
                    }
                    
                    // Multiplie T par alpha A, puis accumule dans resultat
                    CvInvoke.Multiply(tmpResult, aA, tmp2);
                    CvInvoke.Accumulate(tmp2, result);
                }
            });
            
            BlendMatrixPremultipliedColorValues(bgrA, bgrB, alphaA, alphaB, out var final, ALPHA_PRECALC_B, bgrAction);
            bgraResult = final;
        }
    }
}