using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace LeituraWebCorreios
{

    public enum TipoCEP
    {
        LocalidadeLogradouro = 1,
        CEPPromocional = 2, CaixaPostalComunitaria = 3,
        GrandeUsuario = 4, UnidadeOperacional = 5, Todos = 6
    }

    public class CEPCorreios
    {
        public String Logradouro { get; set; }
        public String Bairro { get; set; }
        public String Localidade { get; set; }
        public String UF { get; set; }
        public String CEP { get; set; }
    }

    public class ConsultaCorreios
    {
        private String _retornoHtml = "";
        private CookieContainer cookieContainer = new CookieContainer();

        #region ConsultaCEP
        public void ConsultaCEP(String Texto, TipoCEP tipo, Boolean Semelhanca)
        {
            String parametros;
            string TextoEntrada = Texto;

            parametros = "relaxation=&" +
                         "TipoCep=&" +
                         "semelhante=&" +
                         "cfm=1&" +
                         "Metodo=listaLogradouro&" +
                         "TipoConsulta=relaxation&" +
                         "StartRow=1&" +
                         "EndRow=100";

            TextoEntrada = TextoEntrada.Replace("  ", " ").Replace(" ", "+").Replace("\n", "").Replace("\r", "");
            parametros = parametros.Replace("relaxation=&", "relaxation=" + TextoEntrada + "&");

            switch (tipo){
                case TipoCEP.LocalidadeLogradouro: { parametros = parametros.Replace("TipoCep=&", "TipoCep=LOG&"); break; }
                case TipoCEP.CEPPromocional: { parametros = parametros.Replace("TipoCep=&", "TipoCep=PRO&"); break; }
                case TipoCEP.CaixaPostalComunitaria: { parametros = parametros.Replace("TipoCep=&", "TipoCep=CPC&"); break; }
                case TipoCEP.GrandeUsuario: { parametros = parametros.Replace("TipoCep=&", "TipoCep=GRU&"); break; }
                case TipoCEP.UnidadeOperacional: { parametros = parametros.Replace("TipoCep=&", "TipoCep=UOP&"); break; }
                case TipoCEP.Todos: { parametros = parametros.Replace("TipoCep=&", "TipoCep=ALL&"); break; }
            }

            if (Semelhanca)
                parametros = parametros.Replace("semelhante=&", "semelhante=S&");
            else
                parametros = parametros.Replace("semelhante=&", "semelhante=N&");

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.buscacep.correios.com.br/servicos/dnec/consultaEnderecoAction.do");
            httpWebRequest.CookieContainer = cookieContainer;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 20000;
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.ContentLength = parametros.Length;

            try{
                StreamWriter stParametros = new StreamWriter(httpWebRequest.GetRequestStream(), Encoding.GetEncoding("ISO-8859-1"));
                stParametros.Write(parametros);
                stParametros.Close();
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK){
                    StreamReader stHtml = new StreamReader(httpWebRequest.GetResponse().GetResponseStream(), Encoding.GetEncoding("ISO-8859-1"));
                    _retornoHtml = stHtml.ReadToEnd();
                    stHtml.Close();
                }
            }
            catch (Exception ex){
                _retornoHtml = ex.Message;
            }
        }
        #endregion

        #region Retorno em HTML
        internal String RetornoHTML(){
            return _retornoHtml;
        }
        #endregion

        #region Retorno em Lista de Registros
        public List<CEPCorreios> RetornoCEP(){

            List<CEPCorreios> list = new List<CEPCorreios>();
            int PosicaoLinha, PosicaoColuna, numeroColuna;
            String Coluna, strColunas, strLinha, Col1, Col2, Col3, Col4, Col5;

            String str = _retornoHtml.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("\\", "");             
            str = StringSaltaString(str, "<!-- Fim cabecalho da tabela -->");
            str = StringEntreString(str, "<table", "</table");                        
            PosicaoLinha = 0;
            
            while (str.Contains("tr>")){
                PosicaoLinha = str.IndexOf("/tr>") + 4;   
                strLinha = StringEntreString(str, "<tr", "/tr>");
                strColunas = strLinha;
                Coluna = "";                
                numeroColuna = 0;
                Col1 = null; Col2 = null; Col3 = null; Col4 = null; Col5 = null;

                while (strColunas.Contains("td>")){
                    PosicaoColuna = strColunas.IndexOf("/td>") + 3;
                    Coluna = StringEntreString(strColunas, "<td", "/td>");
                    while (Coluna.Contains("<td")) {
                        Coluna = Coluna.Substring(Coluna.IndexOf("<td") + 3);
                    }
                    Coluna = StringEntreString(Coluna, ">", "<");
                    Coluna = Coluna.Replace("<", "").Replace(">", "");
                    if (PosicaoColuna < strColunas.Length)
                        strColunas = strColunas.Substring(PosicaoColuna);
                    else
                        strColunas = "";                    
                    numeroColuna++;                    
                    if (numeroColuna == 1) Col1 = Coluna;
                    if (numeroColuna == 2) Col2 = Coluna;
                    if (numeroColuna == 3) Col3 = Coluna;
                    if (numeroColuna == 4) Col4 = Coluna;
                    if (numeroColuna == 5) Col5 = Coluna;
                }

                while ((Col5 == "") || (Col5 == null)){
                    Col5 = Col4; Col4 = "";
                    Col4 = Col3; Col3 = "";
                    Col3 = Col2; Col2 = "";
                    Col2 = Col1; Col1 = "";
                    if ((Col5 == "") && (Col4 == "") && (Col3 == "") && (Col2 == "") && (Col1 == ""))
                        break;
                }

                if ((Col5 != "") || (Col4 != "") || (Col3 != "") || (Col2 != "") || (Col1 != ""))
                list.Add(new CEPCorreios { Logradouro = Col1, Bairro = Col2, Localidade = Col3, UF = Col4, CEP = Col5 });

                if (PosicaoLinha < str.Length)
                    str = str.Substring(PosicaoLinha);
                else
                    str = "";
            }
            return list;
        }
        #endregion

        private String StringEntreString(String Str, String StrInicio, String StrFinal)
        {
            int Ini;
            int Fim;
            int Diff;
            Ini = Str.IndexOf(StrInicio);
            Fim = Str.IndexOf(StrFinal);
            if (Ini > 0) Ini = Ini + StrInicio.Length;
            if (Fim > 0) Fim = Fim + StrFinal.Length;
            Diff = ((Fim - Ini) - StrFinal.Length);
            if ((Fim > Ini) && (Diff > 0) && (Ini > 0))
                return Str.Substring(Ini, Diff);
            else
                return "";
        }

        private String StringSaltaString(String Str, String StrInicio)
        {
            int Ini;
            Ini = Str.IndexOf(StrInicio);
            if (Ini > 0)
            {
                Ini = Ini + StrInicio.Length;
                return Str.Substring(Ini);
            }
            else
                return Str;
        }

    }
}
