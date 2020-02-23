﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Web.Data.Entities
{
    public class UserGroupEntity
    {
        public int Id { get; set; }
        public UserEntity User { get; set; }
        public ICollection<UserEntity> Users { get; set; }

    }
}
