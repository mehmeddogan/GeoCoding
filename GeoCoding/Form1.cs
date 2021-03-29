using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net.Http;
using System.Net;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace GeoCoding
{
    public partial class Form1 : Form
    {
        SqlConnection baglanti;
        SqlCommand komut;
        SqlDataAdapter da;

        public Form1()
        {
            InitializeComponent();
            baglanti = new SqlConnection("Data Source=MEMMED\\SQLEXPRESS;Initial Catalog=GeoCoding;Integrated Security=True");
        }
        void AdresGetir()
        {
            baglanti.Open();
            da = new SqlDataAdapter("SELECT * FROM Address", baglanti);
            DataTable tablo = new DataTable();
            da.Fill(tablo);
            dataGridView1.DataSource = tablo;
            baglanti.Close();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            AdresGetir();
           
        }
        void Coordinate(string enlem, string boylam)
        {
            baglanti.Open();
            string sorgu = "INSERT INTO Coordinate(Enlem,Boylam) VALUES (@Enlem,@Boylam)";
            komut = new SqlCommand(sorgu, baglanti);
            komut.Parameters.AddWithValue("@Enlem", enlem);
            komut.Parameters.AddWithValue("@Boylam", boylam);
            komut.ExecuteNonQuery();
            baglanti.Close();
        }
        void CoordinateSil(String enlem, String boylam)
        {
            string sorgu = "DELETE FROM Coordinate WHERE @Enlem=Enlem AND @Boylam=Boylam";
            komut = new SqlCommand(sorgu,baglanti);
            komut.Parameters.AddWithValue("@Enlem", enlem);
            komut.Parameters.AddWithValue("@Boylam", boylam);
            baglanti.Open();
            komut.ExecuteNonQuery();
            baglanti.Close();
            AdresGetir();
        }
        private async void btn_Find_Click(object sender, EventArgs e)
        {
            if (txt_Enlem.Text == "" || txt_Boylam.Text == "")
            {
                MessageBox.Show("Bos alan bırakmayınız");
            }
            else
            {
                double Enlem, Boylam;
                Enlem = Convert.ToDouble(txt_Enlem.Text);
                Boylam = Convert.ToDouble(txt_Boylam.Text);
                Coordinate(txt_Enlem.Text, txt_Boylam.Text);
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://maps.googleapis.com/");
                HttpResponseMessage response = await client.GetAsync("maps/api/geocode/xml?latlng=" + txt_Enlem.Text + "," + txt_Boylam.Text + "&key=AIzaSyADd2ntBd2tlefA5W9BYb4ymnZWSuG2iWU");
                String result = await response.Content.ReadAsStringAsync();
                List<String> addresses = new List<String>();

                using (StreamReader reader = new StreamReader(await response.Content.ReadAsStreamAsync(), Encoding.UTF8))
                {
                    DataSet dsResult = new DataSet();
                    dsResult.ReadXml(reader);
                    try
                    {
                        foreach (DataRow row in dsResult.Tables["result"].Rows)
                        {
                            string fullAddress = row["formatted_address"].ToString();
                            //Console.WriteLine(fullAddress);
                            addresses.Add(fullAddress);
                        }
                    }
                    catch (Exception)
                    {

                    }

                }
                if (addresses.Count != 0)
                {
                    baglanti.Open();
                    foreach (String address in addresses)
                    {
                        string sorgu = "INSERT INTO Address(Enlem,Boylam,Adres) VALUES (@Enlem,@Boylam,@Adres)";
                        komut = new SqlCommand(sorgu, baglanti);
                        komut.Parameters.AddWithValue("@Enlem", txt_Enlem.Text);
                        komut.Parameters.AddWithValue("@Boylam", txt_Boylam.Text);
                        komut.Parameters.AddWithValue("@Adres", address);
                        komut.ExecuteNonQuery();
                    }
                    baglanti.Close();
                    AdresGetir();
                    CoordinateSil(txt_Enlem.Text, txt_Boylam.Text);
                }
                else
                {
                    MessageBox.Show("Koordinat Verileri Hatalı.En fazla ikinci rakamdan sonra nokta koyulmalı ve noktadan sonra en fazla 6 tane deger girilmelidir!!!\n Örn:\n Enlem=XX.XXXXXX\n Boylam=XX.XXXXXX");
                    CoordinateSil(txt_Enlem.Text, txt_Boylam.Text);
                }
            }
        }
    }
    
    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    

}
