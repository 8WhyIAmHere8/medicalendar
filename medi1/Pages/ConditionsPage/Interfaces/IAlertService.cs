using System.Threading.Tasks;

public interface IAlertService
{
    Task ShowAlert(string title, string message, string cancel);
}