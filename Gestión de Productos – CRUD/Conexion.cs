using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Gestión_de_Productos___CRUD
{
    internal class Conexion
    {
        SqlConnection con;

        public SqlConnection Conectar()
        {
            con = new SqlConnection("Data Source=localhost;Initial Catalog=BDProductos;Integrated Security=True;Connect Timeout=30");
            con.Open();
            return con;
        }

        public void Cerrar()
        {
            if (con != null && con.State != ConnectionState.Closed)
            {
                con.Close();
            }
        }

    }
}
