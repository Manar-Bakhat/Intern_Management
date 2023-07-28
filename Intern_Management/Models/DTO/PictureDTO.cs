namespace Intern_Management.Models.DTO
{
    public class PictureDTO
    {
        public string? FileName { get; set; } // Le nom du fichier de la photo
        public string? ContentType { get; set; } // Le type de fichier (extension), ex: "image/jpeg"
        public string? Data { get; set; } // Les données binaires de l'image sous forme d'un tableau d'octets (byte array)
    }
}
