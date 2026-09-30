namespace Checkpoint5.Services;

/// <summary>
/// Registra as operações da aplicação em logs/app.log.
/// </summary>
public static class Logger
{
    private static readonly object _lock = new();
    private static readonly string _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    private static readonly string _logFile = Path.Combine(_logDirectory, "app.log");

    public static void Info(string mensagem) => Escrever("INFO", mensagem);

    public static void Error(string mensagem) => Escrever("ERROR", mensagem);

    private static void Escrever(string nivel, string mensagem)
    {
        var linha = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {nivel} - {mensagem}";

        lock (_lock)
        {
            try
            {
                Directory.CreateDirectory(_logDirectory);
                File.AppendAllText(_logFile, linha + Environment.NewLine);
            }
            catch
            {
                // Se o log falhar, a aplicação não deve ser interrompida por causa disso.
            }
        }
    }
}
