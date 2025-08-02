using UnityEngine;
using System.IO;
using System;
using System.IO.Compression;
using Cysharp.Threading.Tasks;

public static class SpriteLoader
{
    public static async UniTask<Sprite> LoadSpriteFromFile(string path, float pixelsPerUnit = 100f)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"File not found at path: {path}");
            return null;
        }

        byte[] imageData = await File.ReadAllBytesAsync(path);
        Texture2D texture = new Texture2D(2, 2); // size doesn't matter; LoadImage will resize it

        if (!texture.LoadImage(imageData))
        {
            Debug.LogWarning("Failed to load image data into texture.");
            return null;
        }

        byte[] jpgBytes = texture.EncodeToJPG(75);

        if (!texture.LoadImage(jpgBytes))
        {
            Debug.LogWarning("Failed to load image data into texture.");
            return null;
        }

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                             new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    // Serialize a Sprite to compressed Base64 string
    public static async UniTask<string> Serialize(Sprite sprite)
    {
        if (sprite == null || sprite.texture == null)
            return null;

        byte[] jpgBytes = sprite.texture.EncodeToJPG(75);
        byte[] compressedBytes = await Compress(jpgBytes);
        return Convert.ToBase64String(compressedBytes);
    }

    // Deserialize from compressed Base64 string to Sprite
    public static Sprite Deserialize(string base64, float pixelsPerUnit = 100f)
    {
        if (string.IsNullOrEmpty(base64))
            return null;

        try
        {
            byte[] compressedBytes = Convert.FromBase64String(base64);
            byte[] pngBytes = Decompress(compressedBytes);
        

            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(pngBytes))
            {
                Debug.LogWarning("Failed to load image data.");
                return null;
            }

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                 new Vector2(0.5f, 0.5f), pixelsPerUnit);
        } 
        catch
        {
            return null;
        }
    }

    // Brotli compression
    private static async UniTask<byte[]> Compress(byte[] data)
    {
        using (var output = new MemoryStream())
        {
            using (var brotli = new BrotliStream(output, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
                await brotli.WriteAsync(data, 0, data.Length);
            return output.ToArray();
        }
    }

    // Brotli decompression
    private static byte[] Decompress(byte[] data)
    {
        using (var input = new MemoryStream(data))
        using (var brotli = new BrotliStream(input, CompressionMode.Decompress))
        using (var output = new MemoryStream())
        {
            brotli.CopyTo(output);
            return output.ToArray();
        }
    }
}