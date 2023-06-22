﻿using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Office.Interop.Excel;
using ExcelDataReader;

namespace Project
{
    public class ExcelClass
    {


        private static string caminhoDoArquivo;

        public static Task Run(bool PrimeiraVez, string caminhoDoArquivo)
        {
            return Task.Run(async () =>
            {
                await ExcelClass.Start(PrimeiraVez, caminhoDoArquivo);
                ExcelClass.Read();
            });

        }

        private static Task Start(bool PrimeiraVez, string path)
        {
            Excel.Application excelApp = new Excel.Application();
            caminhoDoArquivo = path;
            if (PrimeiraVez)
            {

                // Cria um novo arquivo Excel
                // Adiciona uma nova planilha ao arquivo

                Excel.Worksheet worksheet = excelApp.Workbooks.Add().Sheets.Add();

                worksheet.Name = "Clientes";
                // Salva o arquivo
                worksheet.SaveAs($@"{caminhoDoArquivo}\DB.xlsx");
                caminhoDoArquivo += @"\DB.xlsx";
                excelApp.Quit();
            }
            else
            {
                caminhoDoArquivo += @"\DB.xlsx";
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Adiciona o <paramref name="cliente"/> ao Banco de Dados Excel
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns></returns>
        private static Task AddLine(Cliente cliente)
        {
           var excelApp = new Excel.Application();

            // Abre Planilha para escrita
            File.Copy(caminhoDoArquivo, caminhoDoArquivo + "aux");
            File.Delete(caminhoDoArquivo);
            Workbook workbook = excelApp.Workbooks.Open(caminhoDoArquivo + "aux");
            Worksheet worksheet = workbook.Sheets["Clientes"];
            Range LastRow = worksheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);
            int row = LastRow.Row;

            // Escreve dados na planilha
            worksheet.Cells[row, 1].Value = cliente.Name;                   //nome do cliente
            worksheet.Cells[row, 2].Value = cliente.Telefone.ToStore;       //Telefone do cliente
            worksheet.Cells[row, 3].Value = cliente.Aniversario.Full_Date;  //Aniversario do cliente
            worksheet.Cells[row, 4].Value = cliente.CPF;                    //CPF do cliente
            worksheet.Cells[row, 5].Value = cliente.Endereço.Cep;           //Cep do cliente
            worksheet.Cells[row, 6].Value = cliente.Complemento;            //Complemento do cliente
            worksheet.Cells[row, 7].Value = cliente.Numero;                 //Numero do cliente
            // Salva o arquivo

            worksheet.SaveAs(caminhoDoArquivo);
            workbook.Close();
            excelApp.Quit();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Salva o cliente atual no arquivo
        /// </summary>
        /// <param name="cliente"></param>
        public static void Save(Cliente cliente)
        {
            _ = Task.Run(async () =>
            {
                await ExcelClass.AddLine(cliente);
            });
        }


        /// <summary>
        /// Lê o arquivo excel de clientes, se chamado
        /// </summary>
        public static void Read()
        {
            var stream = new FileStream(caminhoDoArquivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);//tem
            DataBase.Collection = new DataBase.ListaClientes();

            //espera a leitura total do excel
            while (reader.Read())
            {
                //reader.Get{Type}(colum)?.ToString();
                var split = reader.GetString(1).Split(')');
                split[0] = split[0].Replace("(", "").Replace(")", "").Trim();
                split[1] = split[1].Replace(")", "").Replace("-", "").Trim();

                var date = reader.GetString(2).Replace("/", " ").Split(' ');
                date[0] = date[0].Trim();
                date[1] = date[1].Trim();
                DataBase.Collection.Add(new Cliente
                {
                    Name = reader.GetString(0),
                    Telefone = new CellPhone(int.Parse(split[0]), int.Parse(split[1])),
                    Aniversario = new Date(int.Parse(date[0]), int.Parse(date[1])),
                    CPF = reader.GetString(3),
                    Endereço = new CEP(reader.GetString(4)),
                    Complemento= reader.GetString(5),
                    Numero= reader.GetDouble(6).ToString(),
                }); 
            }
            reader.Close();
        }
    }
}
