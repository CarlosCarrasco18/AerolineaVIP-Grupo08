using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class Pasajero
    {
        public string NombreCompleto
        {
            get { return Nombres + " " + Apellidos; }
        }

        public string ValidarDocumento()
        {
            if (TipoDocumentoCodigo == "CEDULA")
            {
                if (!CedulaValida(NumeroDocumento))
                {
                    return "La cédula ingresada no es válida.";
                }
            }

            if (TipoDocumentoCodigo == "PASAPORTE")
            {
                if (NumeroDocumento == null || NumeroDocumento.Length < 5 || NumeroDocumento.Length > 20 || !NumeroDocumento.All(char.IsLetterOrDigit))
                {
                    return "El pasaporte debe tener entre 5 y 20 letras o números.";
                }
            }

            return null;
        }

        public static bool CedulaValida(string cedula)
        {
            if (cedula == null || cedula.Length != 10 || !cedula.All(char.IsDigit))
            {
                return false;
            }

            int provincia = int.Parse(cedula.Substring(0, 2));
            if ((provincia < 1 || provincia > 24) && provincia != 30)
            {
                return false;
            }

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito > 5)
            {
                return false;
            }

            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(cedula[i].ToString());
                if (i % 2 == 0)
                {
                    digito = digito * 2;
                    if (digito > 9)
                    {
                        digito = digito - 9;
                    }
                }
                suma = suma + digito;
            }

            int verificador = (10 - suma % 10) % 10;
            return verificador == int.Parse(cedula[9].ToString());
        }
    }
}
