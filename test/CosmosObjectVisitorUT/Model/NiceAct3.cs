﻿using System.Text.RegularExpressions;
using CosmosObjectVisitorUT.Model;

namespace CosmosObjectVisitorUT.Model
{
    public class NiceAct3
    {
        public Country? Country { get; set; }
        public Country Country2 { get; set; }
        public string CountryString { get; set; }
        public string StringVal { get; set; }
        
        public string RegexExpression { get; set; }
        public Regex Regex { get; set; }
    }
}