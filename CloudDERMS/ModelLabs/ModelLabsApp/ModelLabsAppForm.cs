using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;


namespace ModelLabsApp
{
	public partial class ModelLabsAppForm : Form
	{
		private CIMAdapter adapter = new CIMAdapter();
		private Delta nmsDelta = null;
        private Delta newDelta = new Delta();
        private Delta delta = null;

        public ModelLabsAppForm()
		{
			InitializeComponent();

			InitGUIElements();
		}

		private void InitGUIElements()
		{
			buttonConvertCIM.Enabled = false;
			buttonApplyDelta.Enabled = false;

			comboBoxProfile.DataSource = Enum.GetValues(typeof(SupportedProfiles));
			comboBoxProfile.SelectedItem = SupportedProfiles.PowerTransformer;
			//comboBoxProfile.Enabled = false; //// other profiles are not supported
		}

		private void ShowOpenCIMXMLFileDialog()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Open CIM Document File..";
			openFileDialog.Filter = "CIM-XML Files|*.xml;*.txt;*.rdf|All Files|*.*";
			openFileDialog.RestoreDirectory = true;

			DialogResult dialogResponse = openFileDialog.ShowDialog(this);
			if (dialogResponse == DialogResult.OK)
			{
				textBoxCIMFile.Text = openFileDialog.FileName;
				toolTipControl.SetToolTip(textBoxCIMFile, openFileDialog.FileName);
				buttonConvertCIM.Enabled = true;
				richTextBoxReport.Clear();
			}
			else
			{
				buttonConvertCIM.Enabled = false;
			}
		}

		private void ConvertCIMXMLToDMSNetworkModelDelta()
		{

            ////SEND CIM/XML to ADAPTER
            
            try
			{
                if (textBoxCIMFile.Text == string.Empty)
                {
                    MessageBox.Show("Must enter CIM/XML file.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                string log;
				nmsDelta = null;
                

                using (FileStream fs = File.Open(textBoxCIMFile.Text, FileMode.Open))
				{
                    
                    nmsDelta = adapter.CreateDelta(fs, (SupportedProfiles)(comboBoxProfile.SelectedItem), out log);//deltu sa operacijama, report(string)
					richTextBoxReport.Text = log;
				}

                
				if (nmsDelta != null)
				{
					//// export delta to file
					using (XmlTextWriter xmlWriter = new XmlTextWriter(".\\deltaExport.xml", Encoding.UTF8))
					{
						xmlWriter.Formatting = Formatting.Indented;
						nmsDelta.ExportToXml(xmlWriter);
						xmlWriter.Flush();
					}
				}


               
			}
			catch (Exception e)
			{
				MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}


            /////citanje iz baze
            //List<Model> modeli = new List<Model>();
            //using (SqlConnection _con = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=NMSDelta;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            //{

            //    using (SqlCommand cmd = new SqlCommand("SELECT Id, Name, Blob FROM dbo.Deltas", _con))
            //    {
            //        _con.Open();
            //        using (SqlDataReader reader = cmd.ExecuteReader())
            //        {
            //            // Check is the reader has any rows at all before starting to read.
            //            if (reader.HasRows)
            //            {
            //                // Read advances to the next row.
            //                while (reader.Read())
            //                {
            //                    Model m = new Model();
            //                    // To avoid unexpected bugs access columns by name.
            //                    m.Id = reader.GetInt32(reader.GetOrdinal("Id"));
            //                    m.Name = reader.GetString(reader.GetOrdinal("Name"));
            //                    m.Name = m.Name.Trim();
            //                    m.Offset = reader.GetBytes(reader.GetOrdinal("Blob"), 0, m.Blob, 0, 18000);
            //                    modeli.Add(m);
            //                }
            //            }
            //        }
            //        _con.Close();
            //    }
            //}

            //foreach (var m in modeli)
            //{
            //    if (m.Name.Equals(textBoxCIMFile.Text.Split('\\')[7]))
            //    {
            //        delta = Delta.Deserialize(m.Blob);
            //    }
            //}

            //string name = "";
            //Tuple<bool, long> search = null;
            //if (modeli.Count > 0)
            //{
            //    foreach (var m in modeli)
            //    {                    
            //        if (m.Name.Equals(textBoxCIMFile.Text.Split('\\')[7]))
            //        {
            //            //delta = Delta.Deserialize(m.Blob);
            //            foreach (var rd in nmsDelta.InsertOperations)
            //            {
            //                for (int i = 0; i < rd.Properties.Count - 1; i++)
            //                {
            //                    if (rd.Properties[i].Id.Equals(ModelCode.IDOBJ_MRID))
            //                    {
            //                        search = delta.ContainsDeltaOperation1(DeltaOpType.Insert, rd.Properties[i].PropertyValue.StringValue);
            //                        if (!search.Item1)
            //                        {
            //                            newDelta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
            //                        }
            //                        else
            //                        {
            //                            rd.Id = search.Item2;
            //                            newDelta.AddDeltaOperation(DeltaOpType.Update, rd, true);
            //                        }
            //                    }
            //                }                           

            //            }

            //            foreach (var rd in delta.InsertOperations)
            //            {
            //                if (!nmsDelta.ContainsDeltaOperation(DeltaOpType.Insert, rd.Id))
            //                {
            //                    newDelta.AddDeltaOperation(DeltaOpType.Delete, rd, true);
            //                }
            //            }
            //            //updateovati bazu sa nmsDeltom
            //            //upis u bazu
            //            //if (nmsDelta != null)
            //            //{
            //            //    string queryStmt1 = "INSERT INTO dbo.Deltas(Id, Name, Blob) VALUES(@Id, @Name, @Blob)";

            //            //    using (SqlConnection _con = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=NMSDelta;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            //            //    {

            //            //        using (SqlCommand _cmd = new SqlCommand(queryStmt1, _con))
            //            //        {
            //            //            SqlParameter param = _cmd.Parameters.Add("@Name", SqlDbType.NChar);
            //            //            SqlParameter param1 = _cmd.Parameters.Add("@Id", SqlDbType.Int);
            //            //            SqlParameter param2 = _cmd.Parameters.Add("@Blob", SqlDbType.VarBinary);
            //            //            param.Value = textBoxCIMFile.Text.Split('\\')[5];
            //            //            param1.Value = ++modeli[modeli.Count - 1].Id;
            //            //            param2.Value = nmsDelta.Serialize();
            //            //            _con.Open();
            //            //            _cmd.ExecuteNonQuery();
            //            //            _con.Close();
            //            //        }

            //            //    }
            //            //}
            //            nmsDelta = newDelta;
            //            nmsDelta.Id = 1;
            //            if (nmsDelta != null)
            //            {
            //                //// export delta to file
            //                using (XmlTextWriter xmlWriter = new XmlTextWriter(".\\deltaExport.xml", Encoding.UTF8))
            //                {
            //                    xmlWriter.Formatting = Formatting.Indented;
            //                    nmsDelta.ExportToXml(xmlWriter);
            //                    xmlWriter.Flush();
            //                }
            //            }
            //        }
            //        else
            //        {
            //            //upis u bazu
            //            if (nmsDelta != null)
            //            {
            //                string queryStmt1 = "INSERT INTO dbo.Deltas(Id, Name, Blob) VALUES(@Id, @Name, @Blob)";

            //                using (SqlConnection _con = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=NMSDelta;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            //                {

            //                    using (SqlCommand _cmd = new SqlCommand(queryStmt1, _con))
            //                    {
            //                        SqlParameter param = _cmd.Parameters.Add("@Name", SqlDbType.NChar);
            //                        SqlParameter param1 = _cmd.Parameters.Add("@Id", SqlDbType.Int);
            //                        SqlParameter param2 = _cmd.Parameters.Add("@Blob", SqlDbType.VarBinary);
            //                        param.Value = textBoxCIMFile.Text.Split('\\')[7];
            //                        param1.Value = ++modeli[modeli.Count - 1].Id;
            //                        param2.Value = nmsDelta.Serialize();
            //                        _con.Open();
            //                        _cmd.ExecuteNonQuery();
            //                        _con.Close();
            //                    }

            //                }
            //            }
            //        }

            //    }
            //}
            //else
            //{                
            //    //upis u bazu
            //    if (nmsDelta != null)
            //    {
            //        string queryStmt1 = "INSERT INTO dbo.Deltas(Id, Name, Blob) VALUES(@Id, @Name, @Blob)";

            //        using (SqlConnection _con = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=NMSDelta;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            //        {

            //            using (SqlCommand _cmd = new SqlCommand(queryStmt1, _con))
            //            {
            //                SqlParameter param = _cmd.Parameters.Add("@Name", SqlDbType.NChar);
            //                SqlParameter param1 = _cmd.Parameters.Add("@Id", SqlDbType.Int);
            //                SqlParameter param2 = _cmd.Parameters.Add("@Blob", SqlDbType.VarBinary);
            //                param.Value = textBoxCIMFile.Text.Split('\\')[7];                            
            //                param1.Value = 1;
            //                param2.Value = nmsDelta.Serialize();
            //                _con.Open();
            //                _cmd.ExecuteNonQuery();
            //                _con.Close();
            //            }

            //        }
            //    }
            //}                       

            buttonApplyDelta.Enabled = (nmsDelta != null);
            textBoxCIMFile.Text = string.Empty;
		}

		private void ApplyDMSNetworkModelDelta()
		{
			//// APPLY Delta
            if (nmsDelta != null)
            {
                try
                {
                    string log = adapter.ApplyUpdates(nmsDelta);
                    richTextBoxReport.AppendText(log);
                    nmsDelta = null;
                    buttonApplyDelta.Enabled = (nmsDelta != null);
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No data is imported into delta object.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
		}

		
		private void buttonBrowseLocationOnClick(object sender, EventArgs e)
		{
			ShowOpenCIMXMLFileDialog();
		}

		private void textBoxCIMFileOnDoubleClick(object sender, EventArgs e)
		{
			ShowOpenCIMXMLFileDialog();
		}

		private void buttonConvertCIMOnClick(object sender, EventArgs e)
		{
			ConvertCIMXMLToDMSNetworkModelDelta();
		}

		private void buttonApplyDeltaOnClick(object sender, EventArgs e)
		{
			ApplyDMSNetworkModelDelta();
		}

		private void buttonExitOnClick(object sender, EventArgs e)
		{
			Close();
		}

        private void TextBoxCIMFile_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
