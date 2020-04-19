
namespace ObligatorioDA2.Services.Contracts
{
    public interface IImageService
    {
        string SaveImage(string imageName,string image);
        byte[] ReadImage(string imagePath);
    }
}
