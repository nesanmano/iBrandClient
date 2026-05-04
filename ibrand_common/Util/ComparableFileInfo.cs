using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Compareur externe pour les functions dynamique de LINQ.
    /// </summary>
    [PublicAPI]
    public class ComparableFileInfoComparer : IComparer<ComparableFileInfo>
    {
        public int Compare(ComparableFileInfo x, ComparableFileInfo y)
        {
            if (x != null) return x.CompareTo(y);
            if (y?.FileInfoRef != null) return -1;
            return 0; // double null;
        }
    }

    /// <summary>
    /// Encapsule un FileInfo pour permettre des opération de comparaison, en plus des opérations d'égalité.
    /// </summary>
    public class ComparableFileInfo : IComparable<FileInfo>, IComparable<ComparableFileInfo>, IEquatable<FileInfo>, IEquatable<ComparableFileInfo>
    {
        public FileInfo FileInfoRef { get; }

        public FileInfoComparisonEnum ComparisonMethod { get; set; }

        public ComparableFileInfo([NotNull] FileInfo original, FileInfoComparisonEnum compareMethod = FileInfoComparisonEnum.FileName)
        {
            FileInfoRef = original;
            ComparisonMethod = compareMethod;
        }

        public int CompareTo(ComparableFileInfo other)
        {
            if (other?.FileInfoRef == null) return 1;

            switch (ComparisonMethod)
            {
                case FileInfoComparisonEnum.Date:
                    return FileInfoRef.LastWriteTime.CompareTo(other.FileInfoRef.LastWriteTime);
                case FileInfoComparisonEnum.FileName:
                    return string.Compare(FileInfoRef.Name, other.FileInfoRef.Name, StringComparison.InvariantCulture);
                case FileInfoComparisonEnum.FullPath:
                    return string.Compare(FileInfoRef.FullName, other.FileInfoRef.FullName, StringComparison.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int CompareTo(FileInfo other)
        {
            if (other == null) return 1;

            switch (ComparisonMethod)
            {
                case FileInfoComparisonEnum.Date:
                    return FileInfoRef.LastWriteTime.CompareTo(other.LastWriteTime);
                case FileInfoComparisonEnum.FileName:
                    return string.Compare(FileInfoRef.Name, other.Name, StringComparison.InvariantCulture);
                case FileInfoComparisonEnum.FullPath:
                    return string.Compare(FileInfoRef.FullName, other.FullName, StringComparison.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equals(object obj)
        {
            // FileInfo n'est PAS égal avec un autre FileInfo sur exactement le même fichier. Ce qui est incroyablement con....

            switch (obj)
            {
                case FileInfo fi:
                    return string.Equals(FileInfoRef?.FullName, fi.FullName);
                case ComparableFileInfo cfi:
                    return string.Equals(FileInfoRef?.FullName, cfi.FileInfoRef?.FullName);
                default:
                    return false;
            }
        }

        public override string ToString()
        {
            return FileInfoRef?.ToString() ?? "NULL";
        }

        public override int GetHashCode()
        {
            return FileInfoRef != null ? FileInfoRef.FullName.GetHashCode() : 0;
        }

        public bool Equals(FileInfo other)
        {
            return string.Equals(FileInfoRef?.FullName, other?.FullName);
        }

        public bool Equals(ComparableFileInfo other)
        {
            return string.Equals(FileInfoRef?.FullName, other?.FileInfoRef?.FullName);
        }
    }

    public enum FileInfoComparisonEnum
    {
        Date,
        FileName,
        FullPath
    }
}