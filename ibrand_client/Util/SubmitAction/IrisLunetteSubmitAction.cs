using System.Drawing;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe de traitement d'un répondant pour le prohjet pilote de Iris.
    /// <br/>Le traitement consiste à normaliser une image selon deux points, à la placer de façon à ce que un tiers
    /// partis (Iris) puisse faire une transformation rapide en superposant une image toujours au même
    /// endroit, selon un centre donné par les deux points et un dimensionnement avec la valeur de distance réelle entre
    /// les deux points.
    /// </summary>
    public class IrisLunetteSubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transform", 1);
        }

        public string FtpUser
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_user"))
                {
                    return _configData["ftp_user"];
                }

                return null;
            }
        }
        
        public int FtpPort
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_port"))
                {
                    return int.Parse(_configData["ftp_port"]);
                }

                return -1;
            }
        }

        public string FtpPassword
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_pass"))
                {
                    return _configData["ftp_pass"];
                }

                return null;
            }
        }

        public string FtpAdress
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_url"))
                {
                    return _configData["ftp_url"];
                }

                return null;
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new IrisLunetteSubmitActionWorker(this, resp, resp.Id);
        }
        
        public string FtpDir
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_dir"))
                {
                    return _configData["ftp_dir"];
                }

                return null;
            }
        }

        /// <summary>
        /// L'angle, en degrés et en valeur absolue, minimal pour que l'on fasse une rotation sur l'image.
        /// </summary>
        public double AngleMinThreshold
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("angle_min"))
                {
                    return double.Parse(_configData["angle_min"]);
                }

                return 0.0;
            }
        }

        /// <summary>
        /// La valeur d'un pixel en distance réelle, en milimètre.
        /// </summary>
        public double PixelMilimeterValue
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("pixel_mm"))
                {
                    return double.Parse(_configData["pixel_mm"]);
                }

                return 1.0;
            }
        }

        /// <summary>
        /// Représente la coordonnée centrale sur laquelle centrer l'image à partir de deux points.
        /// <br/>représente deux entiers qui représentent le point X-Y du pixel central.
        /// </summary>
        public Point? CenterCoordinateFace
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("center_coordinate_face"))
                {
                    return StringUtil.PointFromStringDefinition(_configData["center_coordinate_face"]);
                }

                return null;
            }
        }

        /// <summary>
        /// Représente la coordonnée centrale sur laquelle centrer l'image à partir de deux points.
        /// <br/>représente deux entiers qui représentent le point X-Y du pixel central.
        /// </summary>
        public Point? CenterCoordinateProfil
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("center_coordinate_side"))
                {
                    return StringUtil.PointFromStringDefinition(_configData["center_coordinate_side"]);
                }

                return null;
            }
        }
    }
}
