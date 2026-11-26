using System;
using System.IO;
using System.Text;
using UnityEngine;

// Isolated file store: no scene objects; injectable directory for editor self-tests.
public sealed class SaveRepository
{
    public const int SchemaVersion = 1;
    public const string GameId = "COINBOUND";
    public string FilePath { get; private set; }
    public string LastError { get; private set; }
    public SaveRepository(string directory) { FilePath = Path.Combine(directory, "save.json"); }
    public SaveData Load(Func<SaveData> defaults, Func<SaveData,bool> validate)
    {
        LastError = null;
        try
        {
            if (!File.Exists(FilePath)) return defaults();
            if(new FileInfo(FilePath).Length>1024*1024) throw new InvalidDataException("Save file too large");
            string json = File.ReadAllText(FilePath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                throw new InvalidDataException("Invalid save JSON");
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null || data.version != SchemaVersion || data.game != GameId || !validate(data))
                throw new InvalidDataException("Invalid or unsupported save data");
            return data;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                                      error is ArgumentException || error is InvalidDataException ||
                                      error is NotSupportedException)
        {
            LastError = error.Message;
            return defaults();
        }
    }
    public bool Write(SaveData data)
    {
        LastError = null;
        string temporary = FilePath + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(temporary, JsonUtility.ToJson(data, true), new UTF8Encoding(false));
            if (!File.Exists(FilePath)) File.Move(temporary, FilePath);
            else
            {
                try { File.Replace(temporary, FilePath, FilePath + ".bak"); }
                catch (Exception error) when (error is PlatformNotSupportedException || error is IOException)
                {
                    // Some browser/mobile file systems do not implement File.Replace.
                    File.Copy(temporary, FilePath, true);
                    File.Delete(temporary);
                }
            }
            return true;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                                      error is ArgumentException || error is NotSupportedException)
        {
            LastError = error.Message;
            return false;
        }
    }
}
