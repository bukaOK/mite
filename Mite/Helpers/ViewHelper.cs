using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Autofac.Integration.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Entities;
using Mite.Models;
using System.Web;

namespace Mite.Helpers
{
    public static class ViewHelper
    {
        public static string GetMonthName(int month)
        {
            var months = new[]
            {
                "Январь", "Февраль", "Март", "Апрель", "Май",
                "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
            };
            return months[month - 1];
        }

        /// <summary>
        /// Возвращает падеж слова по числу, последнее слово в массиве должно соответствовать всем остальным до цифры 9
        /// </summary>
        /// <param name="num"></param>
        /// <param name="word1">падеж для 1</param>
        /// <param name="word2">падеж для 2,3,4</param>
        /// <param name="word0">падеж для 0,5,6,7,8,9</param>
        /// <example>3 работы, 5 подписчиков</example>
        /// <returns></returns>
        public static string GetWordCase(int num, string word1, string word2, string word0)
        {
            if(num >= 10 && num <= 20)
            {
                return word0;
            }
            //получаем последнюю цифру
            num = num % 10;

            switch (num)
            {
                case 0:
                    return word0;
                case 1:
                    return word1;
                case 2:
                case 3:
                case 4:
                    return word2;
                default:
                    goto case 0;
            }
        }

        public static List<SelectListItem> GetSelectList(Type enumType) 
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Тип не является перечислением");
            }
            return EnumHelper.GetSelectList(enumType).ToList();
        }

        /// <summary>
        /// Преобразует разницу во времени в слова(два дня назад, неделю назад и тд)
        /// </summary>
        /// <param name="publicTime">Время публикации</param>
        /// <param name="userTime">Время текущего пользователя</param>
        /// <returns></returns>
        public static string GetPastTense(DateTime publicTime, DateTime userTime)
        {
            var difTime = userTime - publicTime;

            const string backWord = "";

            if (difTime.Days >= 365)
            {
                var years = difTime.Days / 365;
                var wordCase = GetWordCase(years, "год", "года", "лет");
                if (years > 1)
                    return $"{years} {wordCase} {backWord}";
                return wordCase + backWord;
            }
            if (difTime.Days >= 30 && difTime.Days < 365)
            {
                var months = difTime.Days / 30;
                var wordCase = GetWordCase(months, "месяц", "месяца", "месяцев");
                if (months > 1)
                    return $"{months} {wordCase} {backWord}";
                return wordCase + backWord;
            }
            if (difTime.Days >= 7 && difTime.Days < 30)
            {
                var weeks = difTime.Days / 7;
                var wordCase = GetWordCase(weeks, "неделя", "недели", "недель");
                if (weeks > 1)
                {
                    return $"{weeks} {wordCase} {backWord}";
                }
                return wordCase + backWord;
            }
            if (difTime.Days >= 1 && difTime.Days < 7)
            {
                var wordCase = GetWordCase(difTime.Days, "день", "дня", "дней");
                if (difTime.Days == 1)
                {
                    return "Вчера";
                }
                if (difTime.Days == 2)
                    return "Позавчера";
                return $"{difTime.Days} {wordCase} {backWord}";
            }
            if (difTime.Hours >= 1 && difTime.Hours < 24)
            {
                var wordCase = GetWordCase(difTime.Hours, "час", "часа", "часов");
                if (difTime.Hours > 1)
                {
                    return $"{difTime.Hours} {wordCase} {backWord}";
                }
                return wordCase + backWord;
            }
            if (difTime.Minutes >= 1 && difTime.Minutes < 60)
            {
                var wordCase = GetWordCase(difTime.Minutes, "минута", "минуты", "минут");
                if (difTime.Minutes > 1)
                {
                    return $"{difTime.Minutes} {wordCase} {backWord}";
                }
                return wordCase + backWord;
            }
            if (difTime.Seconds < 60)
            {
                var wordCase = GetWordCase(difTime.Seconds, "секунда", "секунды", "секунд");
                if (difTime.Seconds > 1)
                {
                    return $"{difTime.Seconds} {wordCase} {backWord}";
                }
                return wordCase + backWord;
            }
            return null;
        }
    }
}