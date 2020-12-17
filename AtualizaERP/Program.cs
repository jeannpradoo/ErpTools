﻿using AtualizaERP.Telas;
using System;
using System.Linq;
using System.Windows.Forms;
using AtualizaERP.Classes;
using System.Text.RegularExpressions;
using System.IO;

namespace AtualizaERP
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string[] argumentos = args;

            //string[] argumentos = {"Download", "http://www.controllernet.com.br/controllernovo/download/Arquivos/VersoesERP/5.2019/Controller520190900_02072019.exe", @"C:\Users\aecio.miranda\Downloads", "Controller520190900_02072019.exe", "T"};

            //string[] argumentos = {"Backup", "B", "AECIO_TESTE", "SA", "CONTROLLER.4000", "SRVDEV00", @"\\SRVDEV00\Backup\TESTE_ERP", @"C:\temp\TesteBD" };

            //string[] argumentos = { "Backup", "R", "AECIO_TESTE", "SA", "CONTROLLER.4000", "SRVDEV00", @"\\SRVDEV00\Backup\TESTE_ERP", @"C:\temp\TesteBD" };

            //string[] argumentos = { "Versao", "128", "520190100", "5.2019.01.00", "08/03/2019", "", "", "", "0" };

            //string[] argumentos = { "Home", "42705001", "17.236.676/0001-81", "002", "0", ""};

            //string[] argumentos = { "Atualiza", "C"};

            //string[] argumentos = { "ScriptSQL", "RebuildIDX", "005" };

            //string[] argumentos = { "ScriptSQL", "VerificaBD", "001" };

            //string[] argumentos = { "Agenda", "MASTER", "15/09/2020 23:49:00", "7", "001" };

            //string[] argumentos = { "Alerta", "Tempo Restante", "Mensagem Parte 1", "Mensagem Parte 2", "Usuario", "IdConexao", "DataInicioTarefa" };

            //string[] argumentos = { "DelAgenda", "Controller ERP - Rebuild_Idx_B" };

            string caminho = "";
            string param = "";
            CriaTarefa tarefa;
            AcessoDados Dados = new AcessoDados();

            Dados.nomeErro = @"C:\Temp\argumentosRecebidos.txt";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            for (int i = 0; i < argumentos.Length; i++)
            {
                param += "Arugumento: " + i.ToString() + " = " + argumentos[i] + "\n";
            }

            Dados.GravaErro(param);
            //MessageBox.Show(param, "Controller ERP");

            switch(argumentos[0])
            {
                case "Alerta": //Argumentos - 1:Tempo Restante; 
                    int tmpRest = 0;
                    DateTime dataIniTask = DateTime.Now;

                    if (!string.IsNullOrEmpty(argumentos[1]))
                        tmpRest = Convert.ToInt32(argumentos[1]);

                    if (!string.IsNullOrEmpty(argumentos[6])) //Data Inicio da Tarefa
                        dataIniTask = Convert.ToDateTime(argumentos[6]);

                    Application.Run(new ExibeAlerta(tmpRest, argumentos[2], argumentos[3], argumentos[4], argumentos[5], dataIniTask, 0));
                    break;

                case "Atualiza": //Argumentos - 1:VersaoCli; 2:VersaoAtual; 3:IDConexao; 4:Opção
                    //Application.Run(new InfoUpdate(argumentos[1]));
                    break;
                case "Home": //Argumentos - 1:VersaoCli;   2:CNPJ Cliente;  3:IDConexao;    4:Opção
                    Application.Run(new Home(argumentos[1], argumentos[2], argumentos[3], argumentos[4], argumentos[5]));
                    break;
                case "Download":

                    caminho = argumentos[2] + "\\" + argumentos[3];

                    if (!File.Exists(caminho) || argumentos[4] == "T") //Se não Existe
                        Application.Run(new DownloadERP(argumentos[1], argumentos[2], argumentos[3], argumentos[4]));
                                        
                    if (argumentos[4] != "T")
                        Application.Run(new Home(argumentos[5], argumentos[6], argumentos[7], argumentos[8], caminho));

                    break;

                case "Backup": //Argumentos -      1:Back/Rest     2: Banco;     3: Usuário;    4: Senha;     5: Servidor;  6:Pasta Backup; 7: Pasta Destino
                    Application.Run(new BackupERP(argumentos[1], argumentos[2], argumentos[3], argumentos[4], argumentos[5], argumentos[6], argumentos[7]));
                    break;

                case "Versao": //                   idVersao,    codVersao,     descVersao,    dataVersao,     impactbd,      urlversao,     urlrelease,    caso
                    Application.Run(new VersaoERP(argumentos[1], argumentos[2], argumentos[3], argumentos[4], argumentos[5], argumentos[6], argumentos[7], argumentos[8]));
                    break;

                case "ScriptSQL": //Exemplo Chamada: string[] argumentos = { "ScriptSQL", "RebuildIDX", "005"};
                                        
                    DirectoryInfo dir;
                    caminho = @"C:\Temp"; //Cria a Pasta Temp
                    if (!Directory.Exists(caminho))
                    {
                        dir = Directory.CreateDirectory(caminho);
                    }

                    caminho = @"C:\Controller\DebugRebuildIdx"; //Cria a Pasta de Debug
                    if (!Directory.Exists(caminho))
                    {
                        dir = Directory.CreateDirectory(caminho);
                    }

                    if (argumentos[1] == "RebuildIDX")
                    {                        
                        if (!string.IsNullOrEmpty(argumentos[2])) //IDConex
                        {
                            var conERP = Dados.LeConfTXT(argumentos[2]);

                            var result = Dados.fechaConBD(conERP, 1); //Single User - Derruba as Conexões
                            if (result == "OK")
                            {
                                Dados.ManDBERP(argumentos[2]);                                
                            }

                            result = Dados.fechaConBD(conERP, 0); //Multi User
                        }
                    }

                    if (argumentos[1] == "VerificaBD")
                    {
                        var conectado = Dados.VerConnDB(argumentos[2]);
                        //MessageBox.Show("Conectado: " + conectado.ToString());
                        if (conectado)
                        {
                            //Deleta a Task criada.
                            tarefa = new CriaTarefa("VerConnBD");
                            var tarefaDel = tarefa.DeleteTask();
                            Application.Run(new ExibeAlerta(0, "Sistema OnLine : )", "As Manutenções no Banco de Dados já foram concluídas e você já pode acessar o sistema normalmente!\n\nBom Trabalho!", "", "", DateTime.Now, 1));
                        }
                    }

                    break;

                case "Agenda": //Argumentos(1:Usuario, 2:DataInicio, 3:Frequencia, 4:ID da Conexão)

                    int freqTarefa = 0;
                    DateTime dataTarefa = DateTime.Now;

                    try
                    {
                        if (!string.IsNullOrEmpty(argumentos[2])) //Data Inicio da Tarefa
                            dataTarefa = Convert.ToDateTime(argumentos[2]);

                        if (!string.IsNullOrEmpty(argumentos[3]))
                            freqTarefa = Convert.ToInt32(argumentos[3]);

                        //Cria a Tarefa
                        tarefa = new CriaTarefa(argumentos[1], dataTarefa, freqTarefa, argumentos[3]);

                        var tarefaok = tarefa.AgendaRebuild();

                        Dados.nomeErro = @"C:\Controller\Complementos\AtualizaERP\RespAtualizaERP.txt";
                        if (tarefaok)
                            Dados.GravaErro("Tarefa Criada e Programada com Sucesso!!!");
                        else
                            Dados.GravaErro("Erro");
                    }
                    catch (Exception ex)
                    {
                        Dados.nomeErro = "";
                        Dados.GravaErro(ex.Message + "\n" + ex.InnerException);
                    }

                    break;

                case "DelAgenda": //Argumentos(1:NomeTarefa) - Deleta a Tarefa

                    try
                    {
                        if (!string.IsNullOrEmpty(argumentos[1]))
                        {
                            
                            tarefa = new CriaTarefa(argumentos[1]);

                            var tarefaok = tarefa.DeleteTask();

                            Dados.nomeErro = @"C:\Controller\Complementos\AtualizaERP\RespAtualizaERP.txt";
                            if (tarefaok)
                                Dados.GravaErro("Tarefa Eliminada com Sucesso!!!");
                            else
                                Dados.GravaErro("Erro");                        
                        }
                        else
                        {
                            Dados.nomeErro = "";
                            Dados.GravaErro("Necessário informar o nome da tarefa que será eliminada!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Dados.nomeErro = "";
                        Dados.GravaErro(ex.Message + "\n" + ex.InnerException);
                    }

                    break;

            } //Switch

            Application.Exit();

        }
    }

}
