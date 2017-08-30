using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LeituraWebCorreios
{
    public partial class Form1 : Form
    {
        public Form1(){
            InitializeComponent();
        }

        private void btnConsulta_Click(object sender, EventArgs e){
            ConsultaCorreios cons = new ConsultaCorreios();
            cons.ConsultaCEP(textEdit1.Text, TipoCEP.Todos, false);            
            gridControl1.DataSource = cons.RetornoCEP();            
        }
    }
}
