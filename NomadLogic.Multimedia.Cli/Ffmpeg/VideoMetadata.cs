using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    /// <summary>
    /// Structure pour visualiser facilement les métadonnées d'un vidéo.
    /// Obtenue en utilsant l'exécutable "ffprobe", qui fait partie du lot venant avec FFMPEG.
    /// Ces données sont lue à partir d'un fichier texte de format propriétaire généré par "ffprobe".
    /// La lecture de ce fichier fait partie de cette classe.
    /// </summary>
    [PublicAPI]
    public class VideoMetadata : IEquatable<VideoMetadata>
    {
        /// <summary>
        /// Liste des flux contenus dans le fichier.
        /// Normalement, le premier flux est celui vidéo, le deuxième celui de l'audio.
        /// Mais cela peut être hautement variable.
        /// </summary>
        public IList<StreamData> Streams { get; set; }

        /// Retourne le plus haut nombre de trame de tous les flux vidéo contenu dans les métadonnées.
        /// Retourne -1 si aucun flux vidéo n'existe, même si des flux audio peuvent contenir cette information.
        public long GetVideoFrameCount()
        {
            return Streams?.Max(s => (long)s.FrameCount) ?? -1;
        }
        
        public static VideoMetadata FromTextFile(FileInfo dataTextFile)
        {
            if (dataTextFile == null || !File.Exists(dataTextFile.FullName))
            {
                return null;
            }

            var md = new VideoMetadata();

            var lines = File.ReadAllLines(dataTextFile.FullName);

            StreamData stream = null;
            var st = typeof(StreamData);
            var ct = typeof(CodecInfo);
            
            // On parse chaque ligne
            foreach (var line in lines)
            {
                var l = line.Trim();

                #region Initialisation des groupes de flux.
                if (l.StartsWith("[STREAM]"))
                {
                    stream = new StreamData
                    {
                        Codec = new CodecInfo()
                    };
                    continue;
                }
                if (l.StartsWith("[/STREAM]"))
                {
                    if (md.Streams == null) md.Streams = new List<StreamData>();
                    md.Streams.Add(stream);
                    stream = null;
                    continue;
                }

                if (stream == null) continue;
                #endregion

                #region Dispositions
                const string DISPO = "DISPOSITION:"; 
                // Une disposition est un message de métadonné associé au flux. Comme on ne les gère pas directement, on ne fait que les stocker.
                if (l.StartsWith(DISPO))
                {
                    AddPropertyCandidateToDictionary(stream.Dispositions, l.Substring(DISPO.Length));
                    continue;
                }
                #endregion

                #region Étiquettes
                const string TAG = "TAG:";
                // Similaire aux dispositions, mais avec une catégorie différente.
                if (l.StartsWith(TAG))
                {
                    AddPropertyCandidateToDictionary(stream.Tags, l.Substring(TAG.Length));
                    continue;
                }
                #endregion

                #region Propriétés
                // Les propriétés les plus importantes/utiles. Suffisamment pour avoir leur propre point d'accès dans la classe.
                var prop = StringUtil.ExtractPropertyFromString(l);
                if (prop == null) continue;
                switch (prop[0])
                {
                    case "index" :
                        if (int.TryParse(prop[1], out var index)) stream.Index = index;
                        continue;
                    case "codec_name" :
                        stream.Codec.CodecShortName = prop[1];
                        continue;
                    case "codec_long_name" :
                        stream.Codec.CodecFullName = prop[1];
                        continue;
                    case "codec_type" :
                        if (Enum.TryParse(prop[1], out CodecTypeEnum val)) stream.Codec.CodecType = val;
                        continue;
                    case "codec_time_base" :
                        TryAssignFractionToProperty(prop[1], ct.GetProperty("CodecTimeBase"), stream.Codec);
                        continue;
                    case "codec_tag_string" :
                        stream.Codec.CodecTag = prop[1];
                        continue;
                    case "codec_tag" :
                        stream.Codec.CodecTagHex = prop[1];
                        continue;
                    case "width" :
                        var oldSizeW = stream.Resolution;
                        if (int.TryParse(prop[1], out var width)) stream.Resolution = new Size(width, oldSizeW?.Height ?? 0);
                        continue;
                    case "height" :
                        var oldSizeH = stream.Resolution;
                        if (int.TryParse(prop[1], out var height)) stream.Resolution = new Size(oldSizeH?.Width ?? 0, height);
                        continue;
                    case "sample_aspect_ratio" :
                        TryAssignFractionToProperty(prop[1], st.GetProperty("PixelAspectRatio"), stream);
                        continue;
                    case "display_aspect_ratio":
                        TryAssignFractionToProperty(prop[1], st.GetProperty("VideoAspectRatio"), stream);
                        continue;
                    case "pix_fmt" :
                        stream.PixelFormat = prop[1];
                        continue;
                    case "avg_frame_rate" :
                        TryAssignFractionToProperty(prop[1], st.GetProperty("FrameRate"), stream);
                        continue;
                    case "start_time" :
                        if (double.TryParse(prop[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var startTime)) stream.StartTime = startTime;
                        continue;
                    case "duration" :
                        if (double.TryParse(prop[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var dur)) stream.DurationSeconds = dur;
                        continue;
                    case "bit_rate" :
                        if (ulong.TryParse(prop[1], out var br)) stream.BitRate = br;
                        continue;
                    case "max_bit_rate" :
                        if (prop[1].Equals("N/A"))
                        {
                            stream.BitRateMax = null;
                        }
                        else if (ulong.TryParse(prop[1], out var mbr))
                        {
                            stream.BitRateMax = mbr;
                        }
                        continue;
                    case "nb_frames" :
                        if (uint.TryParse(prop[1], out var nbFrames)) stream.FrameCount = nbFrames;
                        continue;
                }

                #endregion

                #region Autres
                // Les autres propriétés, que l'on met dans un "grab-bag"
                AddPropertyCandidateToDictionary(stream.Others, l);
                #endregion

            }

            return md;
        }

        private static void TryAssignFractionToProperty(string s, PropertyInfo prop, object instance)
        {
            var sar = StringUtil.FractionFromStringDefinition(s);
            if (sar != null) prop.SetValue(instance, sar, null);
        }
        
        private static bool AddPropertyCandidateToDictionary(IDictionary<string, string> dic, string candidate)
        {
            var prop = StringUtil.ExtractPropertyFromString(candidate);
            if (prop == null) return false;
            if (!dic.ContainsKey(prop[0])) dic.Add(prop[0], prop[1]);
            return true;
        }

        /// Retourne le premier flux video trouvé dans les méta données. Peut être nul.
        public StreamData GetMainVideoStreamData()
        {
            return Streams?.FirstOrDefault(s => s.Codec?.CodecType == CodecTypeEnum.video);
        }
        
        public bool Equals(VideoMetadata other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Streams == null && other.Streams == null) return true; // pseudi null-check. Si les deux sont null on retourne vrai.
            if (Streams == null || other.Streams == null) return false; // Si un faux et l'autre vrai
            return Streams.SequenceEqual(other.Streams); // Si les deux sont non-null, vérification d'égalité de séquence.
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VideoMetadata) obj);
        }

        public override int GetHashCode()
        {
            return (Streams != null ? Streams.GetHashCode() : 0);
        }

        public static bool operator ==(VideoMetadata left, VideoMetadata right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VideoMetadata left, VideoMetadata right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
    
    /// <summary>
    /// Classe de données pour un flux spécifique dans un fichier vidéo.
    /// Les propriétés lues d'un flux sont toutes consignées dans trois dictionnaires différents, sauf si la propriétée est
    /// assez générique et/ou utile à notre système. Auquel cas, elle vont avoir leur propre point d'accès comme propriété C#.
    /// Si le type de la propriété est clair, la valeur sera convertie dans une classe/structure directement. Sinon, la valeur sera conservée
    /// sous format string.
    /// Pour l'instant, regroupe les flux vidéo et audio en une seule classe.
    /// </summary>
    [PublicAPI]
    public class StreamData : IEquatable<StreamData>
    {
        /// Catégorie de propriétés du flux avec "DISPOSITION:" comme préfixe.
        public IDictionary<string, string> Dispositions { get; set; } = new Dictionary<string, string>();
        /// Catégorie de propriétés du flux avec "TAG:" comme préfixe.
        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
        /// Autres propriétés que l'on ne juge pas asser générique pour être mise à l'avant plan dans la classe, mais
        /// qui composent le reste des méta-données importantes du flux. Probablement plus utiles pour un décodeur.
        public IDictionary<string, string> Others { get; set; } = new Dictionary<string, string>();

        /// l'index du flux dans le fichier
        public int Index { get; set; } = -1;
        /// Informations condensées pour le Codec
        public CodecInfo Codec { get; set; } = new CodecInfo();
        /// Les dimensions des trames du video. Pas utilisé dans le cas d'un flux audio.
        public Size? Resolution { get; set; }
        /// Le ratio des pixels dans un video.
        [CanBeNull]
        public Fraction PixelAspectRatio { get; set; }
        /// Le ratio des dimensions. Par exemple: "16:9"
        [CanBeNull]
        public Fraction VideoAspectRatio { get; set; }
        /// Le nom du format de stockage des pixels dans le flux.
        [CanBeNull]
        public string PixelFormat { get; set; }
        /// Le nombre de trames par secondes.
        [CanBeNull]
        public Fraction FrameRate { get; set; }
        /// Le temps de départ du flux. N'est pas toujours zéro, surtout dans le cas de flux audio.
        public double StartTime { get; set; }
        /// Temps en secondes fractionnaires du vidéo. Basé sur le nombre de trames et de leur vitesse d'affichage.
        public double DurationSeconds { get; set; }
        /// Nombre de bits (moyen) lu par lecture sur le flux.
        public ulong BitRate { get; set; }
        /// Nombre maximal de bits lus par sonde, si le flux est variable. Peut être nul si non spécifié par le codec.
        [CanBeNull]
        public ulong? BitRateMax { get; set; }
        /// Nombre de trames complètes stockées dans le vidéo. Le nombre d'images uniques pouvant être créées par la lecture du flux.
        public uint FrameCount { get; set; }

        public bool Equals(StreamData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var test = true;
            test &= Dispositions.SequenceEqual(other.Dispositions); if (!test) return false;
            test &= Tags.SequenceEqual(other.Tags); if (!test) return false;
            test &= Others.SequenceEqual(other.Others); if (!test) return false;
            test &= Index == other.Index; if (!test) return false;
            test &= Equals(Codec, other.Codec); if (!test) return false;
            test &= Equals(Resolution, other.Resolution); if (!test) return false;
            test &= Equals(PixelAspectRatio, other.PixelAspectRatio); if (!test) return false;
            test &= Equals(VideoAspectRatio, other.VideoAspectRatio); if (!test) return false;
            test &= string.Equals(PixelFormat, other.PixelFormat); if (!test) return false;
            test &= Equals(FrameRate, other.FrameRate); if (!test) return false;
            test &= StartTime == other.StartTime; if (!test) return false;
            test &= DurationSeconds == other.DurationSeconds; if (!test) return false;
            test &= BitRate == other.BitRate; if (!test) return false;
            test &= BitRateMax == other.BitRateMax; if (!test) return false;
            return FrameCount == other.FrameCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StreamData)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Dispositions != null ? Dispositions.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tags != null ? Tags.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Others != null ? Others.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Index;
                hashCode = (hashCode * 397) ^ (Codec != null ? Codec.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Resolution.GetHashCode();
                hashCode = (hashCode * 397) ^ (PixelAspectRatio != null ? PixelAspectRatio.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VideoAspectRatio != null ? VideoAspectRatio.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PixelFormat != null ? PixelFormat.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FrameRate != null ? FrameRate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ StartTime.GetHashCode();
                hashCode = (hashCode * 397) ^ DurationSeconds.GetHashCode();
                hashCode = (hashCode * 397) ^ BitRate.GetHashCode();
                hashCode = (hashCode * 397) ^ BitRateMax.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)FrameCount;
                return hashCode;
            }
        }

        public static bool operator ==(StreamData left, StreamData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StreamData left, StreamData right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }

    /// Groupe d'information sur le codec d'un flux.
    [PublicAPI]
    public class CodecInfo : IEquatable<CodecInfo>
    {
        /// Nom court du codec. Example: h264
        public string CodecShortName { get; set; }
        /// Nom complet du codec, pour des fins de présentation.
        public string CodecFullName { get; set; }
        /// Nom de code du codec, en caractère.
        public string CodecTag { get; set; }
        /// Code numérique du codec, sous format de chaîne Hexadécimale.
        public string CodecTagHex { get; set; }
        /// Type de codec.
        public CodecTypeEnum CodecType { get; set; } = CodecTypeEnum.unknown;
        /// Fréquence de saisie du codec. Par example, pour un flux audio de 48 khz, cette valeur est de 1/48000
        [CanBeNull]
        public Fraction CodecTimeBase { get; set; }

        public bool Equals(CodecInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CodecShortName, other.CodecShortName) &&
                string.Equals(CodecFullName, other.CodecFullName) &&
                string.Equals(CodecTag, other.CodecTag) &&
                string.Equals(CodecTagHex, other.CodecTagHex) &&
                CodecType == other.CodecType &&
                Equals(CodecTimeBase, other.CodecTimeBase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CodecInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CodecShortName != null ? CodecShortName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CodecFullName != null ? CodecFullName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CodecTag != null ? CodecTag.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CodecTagHex != null ? CodecTagHex.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)CodecType;
                hashCode = (hashCode * 397) ^ (CodecTimeBase != null ? CodecTimeBase.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(CodecInfo left, CodecInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CodecInfo left, CodecInfo right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("[");
            sb.Append("ShortName=");
            sb.Append(CodecShortName);
            sb.Append(", FullName=");
            sb.Append(CodecFullName);
            sb.Append(", Tag=");
            sb.Append(CodecTag);
            sb.Append(", TagHex=");
            sb.Append(CodecTagHex);
            sb.Append(", Type=");
            sb.Append(CodecType);
            sb.Append(", CodecTimeBase=");
            sb.Append(CodecTimeBase);
            sb.Append("]");

            return sb.ToString();
        }
    }

    /// Types pour l'encodage d'un codec.
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum CodecTypeEnum
    {
        /// Valeur inconnue. Signifit habituellement que la classe n'a pas été msie à jour par un parser.
        unknown,
        /// Encodage pour vidéo
        video,
        /// Encodage pour audio
        audio
    }

    /* Exemple de fichier texte résultant.
     * [STREAM]
        index=0
        codec_name=h264
        codec_long_name=H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10
        profile=High
        codec_type=video
        codec_time_base=1001/60000
        codec_tag_string=avc1
        codec_tag=0x31637661
        width=1920
        height=1080
        coded_width=1920
        coded_height=1088
        has_b_frames=2
        sample_aspect_ratio=0:1
        display_aspect_ratio=0:1
        pix_fmt=yuvj420p
        level=40
        color_range=pc
        color_space=unknown
        color_transfer=unknown
        color_primaries=unknown
        chroma_location=left
        timecode=N/A
        refs=4
        is_avc=true
        nal_length_size=4
        id=N/A
        r_frame_rate=30000/1001
        avg_frame_rate=30000/1001
        time_base=1/30000
        start_pts=0
        start_time=0.000000
        duration_ts=144144
        duration=4.804800
        bit_rate=1524200
        max_bit_rate=N/A
        bits_per_raw_sample=8
        nb_frames=144
        nb_read_frames=N/A
        nb_read_packets=N/A
        DISPOSITION:default=1
        DISPOSITION:dub=0
        DISPOSITION:original=0
        DISPOSITION:comment=0
        DISPOSITION:lyrics=0
        DISPOSITION:karaoke=0
        DISPOSITION:forced=0
        DISPOSITION:hearing_impaired=0
        DISPOSITION:visual_impaired=0
        DISPOSITION:clean_effects=0
        DISPOSITION:attached_pic=0
        TAG:language=eng
        TAG:handler_name=VideoHandler
        [/STREAM]
        [STREAM]
        index=1
        codec_name=aac
        codec_long_name=AAC (Advanced Audio Coding)
        profile=LC
        codec_type=audio
        codec_time_base=1/48000
        codec_tag_string=mp4a
        codec_tag=0x6134706d
        sample_fmt=fltp
        sample_rate=48000
        channels=2
        channel_layout=stereo
        bits_per_sample=0
        id=N/A
        r_frame_rate=0/0
        avg_frame_rate=0/0
        time_base=1/48000
        start_pts=-1024
        start_time=-0.021333
        duration_ts=231654
        duration=4.826125
        bit_rate=125390
        max_bit_rate=128000
        bits_per_raw_sample=N/A
        nb_frames=227
        nb_read_frames=N/A
        nb_read_packets=N/A
        DISPOSITION:default=1
        DISPOSITION:dub=0
        DISPOSITION:original=0
        DISPOSITION:comment=0
        DISPOSITION:lyrics=0
        DISPOSITION:karaoke=0
        DISPOSITION:forced=0
        DISPOSITION:hearing_impaired=0
        DISPOSITION:visual_impaired=0
        DISPOSITION:clean_effects=0
        DISPOSITION:attached_pic=0
        TAG:language=eng
        TAG:handler_name=SoundHandler
        [/STREAM]
     */
}