using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Gestión_de_Productos___CRUD
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Configura el grid para trabajar como una tabla simple de seleccion.
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.MultiSelect = false;
            dgvProductos.ReadOnly = true;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.CellClick += dgvProductos_CellClick;

            CargarProductos();
        }

        // Inserta un nuevo producto en la base de datos y luego refresca el grid.
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;
            InsertarProducto();
        }

        // Busca productos por nombre y muestra el resultado en el DataGridView.
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            BuscarProductos();
        }

        // Vuelve a cargar todos los productos registrados en la base de datos.
        private void btnMostrarTodos_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            CargarProductos();
        }

        // Copia los datos de la fila seleccionada a los TextBox para editar o eliminar.
        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = dgvProductos.Rows[e.RowIndex];

            txtNombre.Text = fila.Cells["Nombre"].Value.ToString();
            txtPrecio.Text = fila.Cells["Precio"].Value.ToString();
            txtCantidad.Text = fila.Cells["Cantidad"].Value.ToString();
        }

        // Actualiza el producto seleccionado en la base de datos.
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;
            ActualizarProducto();
        }

        // Elimina el producto seleccionado en la base de datos.
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            EliminarProducto();
        }

        // Consulta todos los productos y los asigna como origen de datos del grid.
        public void CargarProductos()
        {
            Conexion cn = new Conexion();

            try
            {
                SqlConnection con = cn.Conectar();
                string query = "SELECT Id, Nombre, Precio, Cantidad FROM Productos";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();

                da.Fill(dt);
                dgvProductos.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // Inserta los datos escritos en los TextBox en la tabla Productos.
        public void InsertarProducto()
        {
            Conexion cn = new Conexion();

            try
            {
                SqlConnection con = cn.Conectar();
                string query = "INSERT INTO Productos (Nombre, Precio, Cantidad) VALUES (@Nombre, @Precio, @Cantidad)";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text.Trim());
                cmd.Parameters.AddWithValue("@Precio", decimal.Parse(txtPrecio.Text));
                cmd.Parameters.AddWithValue("@Cantidad", int.Parse(txtCantidad.Text));

                cmd.ExecuteNonQuery();

                MessageBox.Show("Producto agregado correctamente.", "Exito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarProductos();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al insertar el producto: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // Busca productos cuyo nombre coincida total o parcialmente con el texto ingresado.
        public void BuscarProductos()
        {
            Conexion cn = new Conexion();

            try
            {
                SqlConnection con = cn.Conectar();
                string query = "SELECT Id, Nombre, Precio, Cantidad FROM Productos WHERE Nombre LIKE @Buscar";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                cmd.Parameters.AddWithValue("@Buscar", "%" + txtBuscar.Text.Trim() + "%");
                da.Fill(dt);

                dgvProductos.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar productos: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // Actualiza en la base de datos el producto que esta seleccionado en el grid.
        public void ActualizarProducto()
        {
            int idProducto = ObtenerIdSeleccionado();
            if (idProducto == -1) return;

            Conexion cn = new Conexion();

            try
            {
                SqlConnection con = cn.Conectar();
                string query = "UPDATE Productos SET Nombre = @Nombre, Precio = @Precio, Cantidad = @Cantidad WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text.Trim());
                cmd.Parameters.AddWithValue("@Precio", decimal.Parse(txtPrecio.Text));
                cmd.Parameters.AddWithValue("@Cantidad", int.Parse(txtCantidad.Text));
                cmd.Parameters.AddWithValue("@Id", idProducto);

                int filasAfectadas = cmd.ExecuteNonQuery();

                if (filasAfectadas > 0)
                {
                    MessageBox.Show("Producto actualizado correctamente.", "Exito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarProductos();
                    LimpiarCampos();
                }
                else
                {
                    MessageBox.Show("No se encontro el producto para actualizar.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el producto: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // Elimina de la base de datos el producto seleccionado actualmente.
        public void EliminarProducto()
        {
            int idProducto = ObtenerIdSeleccionado();
            if (idProducto == -1) return;

            DialogResult confirmacion = MessageBox.Show(
                "¿Desea eliminar este producto?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            Conexion cn = new Conexion();

            try
            {
                SqlConnection con = cn.Conectar();
                string query = "DELETE FROM Productos WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Id", idProducto);

                int filasAfectadas = cmd.ExecuteNonQuery();

                if (filasAfectadas > 0)
                {
                    MessageBox.Show("Producto eliminado correctamente.", "Exito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarProductos();
                    LimpiarCampos();
                }
                else
                {
                    MessageBox.Show("No se encontro el producto para eliminar.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el producto: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // Obtiene el Id del producto seleccionado para usarlo en actualizar o eliminar.
        private int ObtenerIdSeleccionado()
        {
            if (dgvProductos.CurrentRow == null || dgvProductos.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione un producto de la tabla.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }

            return Convert.ToInt32(dgvProductos.CurrentRow.Cells["Id"].Value);
        }

        // Limpia las cajas de texto despues de guardar, actualizar o eliminar.
        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtPrecio.Clear();
            txtCantidad.Clear();
            txtBuscar.Clear();
            txtNombre.Focus();
        }

        // Valida que los campos requeridos tengan datos correctos antes de enviar a SQL Server.
        private bool ValidarCampos()
        {
            decimal precio;
            int cantidad;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validacion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecio.Text, out precio) || precio < 0)
            {
                MessageBox.Show("El precio debe ser un numero valido.", "Validacion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return false;
            }

            if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad < 0)
            {
                MessageBox.Show("La cantidad debe ser un numero entero valido.", "Validacion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.Focus();
                return false;
            }

            return true;
        }
    }
}
