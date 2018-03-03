﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: May Zin Ko
    public class PendingPurchaseOrderDTO
    {
     
            public int PoNum { get; set; }

            public String Supplier { get; set; }

             [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
                public DateTime CreateDate { get; set; }


            public String OrderEmp { get; set; }

            public double TotalCost { get; set; }
        }
    
}