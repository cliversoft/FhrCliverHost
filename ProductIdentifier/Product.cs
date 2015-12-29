﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Web;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;

namespace Cliver.ProductIdentifier
{
    public enum Field
    {
        Name,
        Description,
        Category
    }

    public class Product
    {
        public readonly Fhr.ProductOffice.Models.Product DbProduct;

        public string Normalized(Field field)
        {
            string t;
            if (!field2text.TryGetValue(field, out t))
            {
                switch (field)
                {
                    case Field.Name:
                        t = get_normalized(DbProduct.Name);
                        break;
                    case Field.Description:
                        t = get_normalized(DbProduct.Description);
                        field2text[field] = t;
                        break;
                    case Field.Category:
                        t = engine.Companies.Get(DbProduct.CompanyId).NormalizedCategory(DbProduct.Category);
                        field2text[field] = t;
                        break;
                    default:
                        throw new Exception("untreated case");
                }
            }
            field2text[field] = t;
            return t;
        }
        readonly Dictionary<Field, string> field2text = new Dictionary<Field, string>();

        internal string get_normalized(string text)
        {
            string t = Cliver.PrepareField.Html.GetDbField(text);
            t = t.ToLower();
            t = engine.Configuration.Get(DbProduct.CompanyId).ReplaceWithSynonyms(t);
            t = engine.Configuration.Get(DbProduct.CompanyId).StripOfIgnoredWords(t);
            t = Regex.Replace(t, @"\s\s+", " ", RegexOptions.Compiled | RegexOptions.Singleline);
            return t.Trim();
        }

        public Dictionary<string, int> Words2Count(Field field)
        {
            Dictionary<string, int> w2c;
            if (!field2word2count.TryGetValue(field, out w2c))
            {
                w2c = new Dictionary<string, int>();
                foreach (Match m in Regex.Matches(Normalized(field), @"\w+|\d+[\:\.]\d+", RegexOptions.Singleline))
                {
                    string word = m.Value.Trim().ToLower();
                    //if(w != w.ToUpper())
                    //string word = Char.ToLowerInvariant(w[0]) + w.Substring(1);
                    word = Regex.Replace(word, "-", "", RegexOptions.Singleline);
                    if (!w2c.ContainsKey(word))
                        w2c[word] = 0;
                    w2c[word]++;
                }
                field2word2count[field] = w2c;
            }
            return w2c;
        }
        readonly Dictionary<Field, Dictionary<string, int>> field2word2count = new Dictionary<Field, Dictionary<string, int>>();

        public Dictionary<string, int>.KeyCollection Words(Field field)
        {
            return Words2Count(field).Keys;
        }

        internal Product(Engine engine, Fhr.ProductOffice.Models.Product product)
        {
            this.engine = engine;
            if (product == null)
                throw new Exception("product is null");
            DbProduct = product;
        }
        readonly Engine engine;
    }

    internal class Products
    {
        internal Products(Engine engine)
        {
            this.engine = engine;
        }
        readonly Engine engine;

        internal Product Get(int product_id)
        {
            Product p = null;
            if (!product_ids2Product.TryGetValue(product_id, out p))
            {
                Fhr.ProductOffice.Models.Product product = engine.Db.Products.Where(x => x.Id == product_id).FirstOrDefault();
                if (product == null)
                    throw new Exception("No Product with Id=" + product_id);
                p = new Product(engine, product);
                product_ids2Product[product.Id] = p;
            }
            return p;
        }

        internal Product[] GetLinked(int link_id)
        {
            List<Product> ps = new List<Product>();
            foreach (Fhr.ProductOffice.Models.Product product in engine.Db.Products.Where(p => p.LinkId == link_id))
            {
                Product p = null;
                if (!product_ids2Product.TryGetValue(product.Id, out p))
                {
                    p = new Product(engine, product);
                    product_ids2Product[product.Id] = p;
                }
                ps.Add(p);
            }
            return ps.ToArray();
        }

        //public static Product Get(int product_id)
        //{
        //    Product p = null;
        //    if (!product_ids2Product.TryGetValue(product_id, out p))
        //    {
        //        Fhr.ProductOffice.Product dp = Fhr.Db.Products.Where(x => x.Id == product_id).FirstOrDefault();
        //        p = new Product(dp);
        //        product_ids2Product[product_id] = p;
        //    }
        //    return p;
        //}

        //public static Product Get(Fhr.ProductOffice.Product product)
        //{
        //    return Get(product.Id);
        //}
        Dictionary<int, Product> product_ids2Product = new Dictionary<int, Product>();

        internal void Ininitalize(ICollection<Fhr.ProductOffice.Models.Product> products)
        {
            foreach (Fhr.ProductOffice.Models.Product product in products)
            {
                if (product == null)
                    throw new Exception("product is null");
                Product p = null;
                if (!product_ids2Product.TryGetValue(product.Id, out p))
                {
                    p = new Product(engine, product);
                    product_ids2Product[product.Id] = p;
                }
            }
        }

        internal List<Product> All
        {
            get
            {
                return product_ids2Product.Values.ToList();
            }
            set
            {
                foreach (Product p in value)
                    product_ids2Product[p.DbProduct.Id] = p;
            }
        }
    }
}
