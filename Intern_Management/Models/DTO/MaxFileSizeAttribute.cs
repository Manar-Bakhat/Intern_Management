using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Intern_Management.Models.DTO
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxFileSizeAndTypeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSizeInBytes;
        private readonly string[] _allowedFileTypes = { "image/jpeg", "image/gif", "image/png" };

        public MaxFileSizeAndTypeAttribute(int maxFileSizeInKb)
        {
            _maxFileSizeInBytes = maxFileSizeInKb * 1024;
        }

        // Disabling CS8765 warning for the IsValid method
#pragma warning disable CS8765
        public override bool IsValid([NotNull] object value)
        {
            if (value is byte[] fileBytes)
            {
                // Validate file size
                if (fileBytes.Length > _maxFileSizeInBytes)
                    return false;

                // Validate file type
                string fileType = GetFileType(fileBytes);
                return _allowedFileTypes.Contains(fileType);
            }

            return false; // Not a valid byte array
        }
#pragma warning restore CS8765

        private string GetFileType(byte[] fileBytes)
        {
            // Check the file header to identify the file type
            string[] jpegHeaders = { "FF", "D8", "FF" };
            string[] gifHeaders = { "47", "49", "46" };
            string[] pngHeaders = { "89", "50", "4E", "47", "0D", "0A", "1A", "0A" };

            if (StartsWithBytes(fileBytes, jpegHeaders))
            {
                return "image/jpeg";
            }
            else if (StartsWithBytes(fileBytes, gifHeaders))
            {
                return "image/gif";
            }
            else if (StartsWithBytes(fileBytes, pngHeaders))
            {
                return "image/png";
            }

            return string.Empty; // Unknown file type
        }

        private bool StartsWithBytes(byte[] sourceArray, string[] headerBytes)
        {
            if (sourceArray.Length < headerBytes.Length)
                return false;

            for (int i = 0; i < headerBytes.Length; i++)
            {
                if (sourceArray[i].ToString("X2") != headerBytes[i])
                    return false;
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be a JPG, GIF, or PNG image and cannot exceed {_maxFileSizeInBytes / 1024} KB.";
        }
    }
}
