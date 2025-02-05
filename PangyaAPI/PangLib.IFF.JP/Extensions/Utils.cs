using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
namespace PangLib.IFF.JP.Extensions
{
    public class Utils
    {
        public Utils() { }
        public static bool IsShiftJIS(string input)
        {
            // Convertendo a string para bytes usando a codificação Shift-JIS
            Encoding sjisEnc = Encoding.GetEncoding("shift_jis");
            byte[] sjisBytes = sjisEnc.GetBytes(input);

            // Verifica se há bytes que correspondem a caracteres Shift-JIS japoneses
            for (int i = 0; i < sjisBytes.Length - 1; i++)
            {
                byte firstByte = sjisBytes[i];
                byte secondByte = sjisBytes[i + 1];

                // Verifica se os bytes correspondem a um caractere Shift-JIS japonês
                if ((firstByte >= 0x81 && firstByte <= 0x9F) ||
                    (firstByte >= 0xE0 && firstByte <= 0xEF && secondByte >= 0x40 && secondByte <= 0xFC))
                {
                    return true;
                }
            }

            return false;
        }

        public static uint GetItemGroup(uint _typeid)
        {
            return (uint)((_typeid & 0xFC000000) >> 26);
        }


        public static string TranslateText(string texto, string idiomaOrigem, string idiomaDestino)
        {

            if (string.IsNullOrEmpty(texto))
                return "";

            string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(texto)}&langpair={idiomaOrigem}|{idiomaDestino}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string json = response.Content.ReadAsStringAsync().Result;
                        dynamic resultado = JObject.Parse(json);
                        string translatedText = resultado.responseData.translatedText;

                        // Mapeamento de substituições desejadas
                        Dictionary<string, string> replacements = new Dictionary<string, string>
                {
                    { "Panya", "Pangya" },
                    { "Panja", "Pangya" },
                    { "Ken", "Nuri" },
                    { "Daisuke", "Azer" },
                    { "Erica", "Hana" },
                    { "Erika", "Hana" },
                    { "Qaz", "Kaz" },
                    { "Kazu", "Kaz" } ,
                      { "\\ c", "\\c" },
                               { " abze", "" },
                                { "abze", "" }
                };

                        // Aplicando as substituições
                        foreach (var replacement in replacements)
                        {
                            translatedText = translatedText.Replace(replacement.Key, replacement.Value);
                        }                                    
                        return translatedText;
                    }
                    else
                    {
                        if (response.IsSuccessStatusCode == false)
                        {

                            string json = response.Content.ReadAsStringAsync().Result;
                            dynamic resultado = JObject.Parse(json);
                        }
                        return texto;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; 
            }
        }
    }
}
