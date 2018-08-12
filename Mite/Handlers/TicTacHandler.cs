using Mite.CodeData.Enums;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace Mite.Handlers
{
    public class TicTacHandler : IHttpHandler
    {
        static readonly int[,] WaysToWin = {
            { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },
            { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },
            { 0, 4, 8 }, { 2, 4, 6 }
        };
        public bool IsReusable => true;

        private const string TrainingDataPath = "~/Files/ticTacTrain.txt";
        private const int MovementsCount = 9;

        private readonly ILogger logger = LogManager.GetLogger("LOGGER");
        private Random random = new Random();
        /// <summary>
        /// Тренировочные данные. Представляют собой набор строк 012345678;-1,
        /// где первая часть последовательность ходов(начиная с крестиков), 
        /// вторая - победитель(1 - крестик, 0 - нолик, -1 - ничья)
        /// </summary>
        private static List<string> TrainingData = File.ReadAllLines(HostingEnvironment.MapPath(TrainingDataPath)).ToList();

        public void ProcessRequest(HttpContext context)
        {
            var req = context.Request;
            //история ходов текущей игры
            var moveSeq = req.QueryString["moveSeq"];
            //За кого играет сервер(cross|zero)
            var aiPlayer = req.QueryString["ai"] == "cross" ? 1 : 0;
            var gameStatus = SearchWinner(moveSeq);

            var resp = context.Response;
            resp.ContentType = "application/json";
            //Если игра закончилась
            if (gameStatus != TicTacWinnerStatus.GameNotOver && gameStatus != TicTacWinnerStatus.Error)
            {
                var existingSeq = TrainingData.FirstOrDefault(x => x.Substring(0, 9) == moveSeq);
                if(existingSeq == null)
                {
                    TrainingData.Add(existingSeq + ";" + aiPlayer);
                    try
                    {
                        File.WriteAllLines(HostingEnvironment.MapPath(TrainingDataPath), TrainingData);
                    }
                    catch(Exception e)
                    {
                        logger.Warn($"TicTac Error: {e.Message}");
                    }
                }
                resp.Write(JsonConvert.SerializeObject(new
                {
                    status = (int)gameStatus
                }));
            }
            else if(gameStatus == TicTacWinnerStatus.Error)
            {
                resp.Write(JsonConvert.SerializeObject(new
                {
                    status = (int)gameStatus
                }));
            }
            //Если игра не закончилась
            else
            {
                if(moveSeq.Length >= 3)
                {
                    var winMove = SearchWinMove(moveSeq, aiPlayer == 1);

                    if(winMove != null)
                    {
                        resp.Write(GetResponse(gameStatus, (int)winMove));
                        return;
                    }
                }
                //Находим последовательности, в которых уже участвовал сервер
                var pcSeqs = TrainingData.Where(x => Regex.IsMatch(x, "^" + moveSeq)).ToList();
                if(pcSeqs.Count > 0)
                {
                    //Находим победную последовательность
                    var winSeq = pcSeqs.Where(x => int.Parse(x.Split(';').Last()) == aiPlayer).ToList();
                    if(winSeq.Count == 0)
                    {
                        //Находим последовательность с ничьей
                        var drawSeq = pcSeqs.Where(x => int.Parse(x.Split(';').Last()) == -1).ToList();
                        
                        if(drawSeq.Count > 0)
                            resp.Write(GetResponse(gameStatus, drawSeq, moveSeq));
                    }
                    else
                    {
                        resp.Write(GetResponse(gameStatus, winSeq, moveSeq));
                    }
                    return;
                }
                for (var i = 0; i < MovementsCount; i++)
                {
                    if (!moveSeq.Contains(i.ToString()))
                    {
                        resp.Write(JsonConvert.SerializeObject(new
                        {
                            status = (int)gameStatus,
                            move = i
                        }));
                        return;
                    }
                }
            }
        }

        private string GetResponse(TicTacWinnerStatus gameStatus, IList<string> sequense, string moveSequense)
        {
            return JsonConvert.SerializeObject(new
            {
                status = (int)gameStatus,
                move = sequense.ElementAt(random.Next(0, sequense.Count - 1))[moveSequense.Length]
            });
        }
        private string GetResponse(TicTacWinnerStatus gameStatus, int move)
        {
            return JsonConvert.SerializeObject(new
            {
                status = (int)gameStatus,
                move
            });
        }
        /// <summary>
        /// Находит победителя в последовательности ходов
        /// </summary>
        /// <param name="moveSeq"></param>
        /// <returns>Статус игры</returns>
        private TicTacWinnerStatus SearchWinner(string moveSeq)
        {
            if (moveSeq.Length > 9)
                return TicTacWinnerStatus.Error;

            var crossMoves = "";
            var zeroMoves = "";
            for(var i = 0; i < moveSeq.Length; i++)
            {
                if (i % 2 == 0)
                    crossMoves += moveSeq[i];
                else
                    zeroMoves += moveSeq[i];
            }
            for(var i = 0; i < WaysToWin.GetLength(0); i++)
            {
                var winMove = "";
                for (var j = 0; j < WaysToWin.GetLength(1); j++)
                    winMove += WaysToWin[i, j];
                //Если в наборе ходов крестика есть набор победителя
                if (crossMoves.Contains(winMove))
                    return TicTacWinnerStatus.Cross;
                //Если в наборе ходов нолика есть набор победителя
                if (zeroMoves.Contains(winMove))
                    return TicTacWinnerStatus.Zero;
            }
            if (moveSeq.Length < 9)
                return TicTacWinnerStatus.GameNotOver;
            else
                return TicTacWinnerStatus.Draw;
        }
        /// <summary>
        /// Возвращает ход, с помощью которого можно победить на следующем ходу, если такого нет, возвращает null
        /// </summary>
        /// <param name="moveSequest">Текущая последовательность ходов</param>
        /// <param name="isCross">Сервер ходит крестиками</param>
        private int? SearchWinMove(string moveSequest, bool isCross)
        {
            if (moveSequest.Length > MovementsCount)
                throw new ArgumentException("moveSequest is too long");

            var pcMoves = "";
            for (var i = 0; i < moveSequest.Length; i++)
                if ((isCross && i % 2 == 0) || (!isCross && i % 2 != 0))
                    pcMoves += moveSequest[i];

            for (var i = 0; i < WaysToWin.GetLength(0); i++)
            {
                var winMove = "";
                for (var j = 0; j < WaysToWin.GetLength(1); j++)
                    winMove += WaysToWin[i, j];
                for(var c = 0; c < MovementsCount; c++)
                {
                    //Добавим к текущей последовательности еще один ход, и проверим, приведет ли он к победе
                    if ((pcMoves + c).Contains(winMove))
                        return c;
                }
            }
            return null;
        }
    }
}