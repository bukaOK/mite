using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mite.DAL.Core;

namespace Mite.BLL.DTO
{
    public class TagDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Подтвержден ли тег
        /// </summary>
        public bool IsConfirmed { get; set; }
        /// <summary>
        /// Проверен ли тег модератором
        /// </summary>
        public bool Checked { get; set; }
        public List<PostDTO> Posts { get; set; }
        public List<UserDTO> Users { get; set; }
    }
    public class TagPopularDTO
    {
        public string Name { get; set; }
        public int? Popularity { get; set; }
    }
}