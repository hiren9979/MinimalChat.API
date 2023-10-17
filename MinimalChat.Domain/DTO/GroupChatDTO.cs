﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.DTO
{
    public class GroupChatDTO
    {
      
        public string Name { get; set; }

        public string CreatorUserId { get; set; }

        public string[] SelectedUserIds { get; set; }
    }
}
