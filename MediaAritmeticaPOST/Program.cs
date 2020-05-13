using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MediaAritmeticaPOST
{
    static class Program
    {
        static void Main(string[] args)
        {
            var lsPessoas = new List<Pessoa>();
            var lsMedia_Cidade_Idade = new List<Media_Cidade_Idade>();
            using (StreamReader sr = new StreamReader(@"C:\Users\Pichau\source\repos\MediaAritmeticaPOST\MediaAritmeticaPOST\input.csv"))
            {
                do
                {
                    string line = sr.ReadLine();
                    string[] arrData = line.Split(',');
                    var pessoa = new Pessoa();

                    lsPessoas.Add(new Pessoa()
                    {
                        nome = RemoveAccents(arrData[0].ToString()).ToUpper().Trim(),
                        cidade = RemoveAccents(arrData[1].ToString()).ToUpper().Trim(),
                        idade = int.Parse(arrData[2])
                    }
                        );
                } while (!(sr.EndOfStream));
            }

            var lsInfoCidade = lsPessoas.GroupBy(g => g.cidade).ToList();

            foreach (var info in lsInfoCidade)
            {
                lsMedia_Cidade_Idade.Add(new Media_Cidade_Idade
                {
                    cidade = info.Key,
                    idade = Convert.ToDouble((info.Sum(s => s.idade) / info.Count()).ToString("n2"))
                });
            }

            var result = new Result();
            result.medias = lsMedia_Cidade_Idade;
            var json = JsonSerializer.Serialize(result);
            var returnPost = ExecutaPost(json);
            Console.WriteLine(returnPost);
            Console.ReadKey();
        }

        public static string ExecutaPost(string message)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                var content = new ByteArrayContent(messageBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var response = client.PostAsync("https://zeit-endpoint.brmaeji.now.sh/api/avg", content).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return $"Exception error: {ex.Message}";
            }
        }

        public static string RemoveAccents(this string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

    }
}
