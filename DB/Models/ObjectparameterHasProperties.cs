﻿using System.ComponentModel;

namespace ResTB.DB.Models
{
    public class ObjectparameterHasProperties
    {
        [ReadOnly(true)]
        [Browsable(false)]
        public int ID { get; set; }
        [ReadOnly(true)]
        [Browsable(false)]
        public Objectparameter Objectparameter { get; set; }
        [ReadOnly(true)]
        public string Property { get; set; }
        [ReadOnly(true)]
        public bool isOptional { get; set; }
    }
}