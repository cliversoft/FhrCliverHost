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
using Cliver.Fhr;

namespace Cliver.ProductIdentifier
{
    public class Engine
    {
        public Engine(bool auto_data_analysing)
        {
            this.Db = Fhr.ProductOffice.Models.DbApi.Create();
            Dbc = Bot.DbConnection.Create(Fhr.ProductOffice.Models.DbApi.GetProviderConnectionString());
            Configuration = new Configuration(this, auto_data_analysing);
            Companies = new Companies(this);
            Products = new Products(this);
            //Words = new Words(this);
        }

        public readonly Configuration Configuration;
        //public Configuration Configuration
        //{
        //    get{return Configuration_;}
        //}
        //Configuration Configuration_;
        internal readonly Companies Companies;
        internal readonly Products Products;
        //internal readonly Words Words;
        internal readonly Cliver.Fhr.ProductOffice.Models.DbApi Db;
        internal readonly Cliver.Bot.DbConnection Dbc;

        //public void RenewConfiguration(bool auto_data_analysing)
        //{
        //    Configuration_ = new Configuration(this, auto_data_analysing);
        //}

        public List<ProductLink> CreateProductLinkList(int[] product1_ids, int company2_id/*, string[] keyword2s = null*/)
        {
            lock (this)
            {
                Fhr.ProductOffice.Models.Product p1 = Db.Products.Where(p => product1_ids.Contains(p.Id) && p.CompanyId == company2_id).FirstOrDefault();
                if (p1 != null)
                    throw new Exception("Product Id:" + p1.Id + " already belongs to company Id:" + p1.CompanyId + " " + p1.Company.Name + " so no more link can be found.");

                //List<int> cis = (from x in Db.Products.Where(p => product1_ids.Contains(p.Id)) join y in Db.Products on x.LinkId equals y.LinkId select y.CompanyId).ToList();
                //cis.Add(company2_id);
                //HashSet<int> cis_ = new HashSet<int>(cis);
                //foreach (int company_id in cis_)
                //{
                //    Configuration.Company c = Configuration.Get(company_id);
                //    if (c.IsDataAnalysisRequired())
                //    {
                //        PerformDataAnalysis(company_id);
                //        c = new Company(this, company_id);
                //        company_ids2Company[company_id] = c;
                //    }
                //}

                Product[] product1s = (from x in product1_ids select Products.Get(x)).ToArray();
                List<ProductLink> pls;
                //if (keyword2s != null && keyword2s.Length > 0)
                //{
                //    keyword2s = keyword2s.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToLower()).ToArray();
                //    HashSet<int> product2_ids = null;
                //    foreach (string keyword2 in keyword2s)
                //    {
                //        HashSet<int> p2_ids = Company.Get(company2_id).Words2ProductIds(Field.Name)[keyword2];
                //        if (product2_ids == null)
                //            product2_ids = p2_ids;
                //        else
                //            product2_ids = (HashSet<int>)product2_ids.Intersect(p2_ids);
                //    }

                //    List<int> link_ids = (from p2_id in product2_ids let link_id = Product.Get(p2_id).DbProduct.LinkId where link_id > 0 group link_id by link_id into g select (int)g.Key).ToList();
                //    pls = (from x in link_ids select new ProductLink(product1s, Product.GetLinked(x))).ToList();

                //    List<int> free_product_ids = (from p2_id in product2_ids let link_id = Product.Get(p2_id).DbProduct.LinkId where link_id == null || link_id <= 0 select p2_id).ToList();
                //    List<ProductLink> pls2 = (from x in free_product_ids select new ProductLink(product1s, new Product[] { Product.Get(x) })).ToList();
                //    pls.AddRange(pls2);
                //}
                //else
                //{
                sw1.Start();
                List<int> link_ids = (from p in Companies.Get(company2_id).DbCompany.Products where p.LinkId > 0 group p by p.LinkId into g select (int)g.Key).ToList();
                pls = (from x in link_ids select new ProductLink(this, product1s, Products.GetLinked(x))).ToList();
                sw2.Start();
                List<int> free_product_ids = (from p in Companies.Get(company2_id).DbCompany.Products where p.LinkId == null || p.LinkId <= 0 select p.Id).ToList();
                sw2.Stop();
                sw3.Start();
                List<ProductLink> pls2 = (from x in free_product_ids select new ProductLink(this, product1s, new Product[] { Products.Get(x) })).ToList();
                sw3.Stop();
                pls.AddRange(pls2);
                //}
                pls = pls.OrderByDescending(x => x.Score).OrderByDescending(x => x.SecondaryScore).ToList();
                sw1.Stop();
                string s = "1: " + sw1.ElapsedMilliseconds + ", 2: " + sw2.ElapsedMilliseconds + ", 3: " + sw3.ElapsedMilliseconds + ", 4: " + sw4.ElapsedMilliseconds
                    + ", 5: " + sw5.ElapsedMilliseconds + ", 6: " + sw6.ElapsedMilliseconds + ", 7: " + sw7.ElapsedMilliseconds
                    + ", 8: " + sw8.ElapsedMilliseconds + ", 9: " + sw9.ElapsedMilliseconds + ", 10: " + sw10.ElapsedMilliseconds;
                return pls;
            }
        }

        public System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw3 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw4 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw5 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw6 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw7 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw8 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw9 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch sw10 = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// Valid LinkId is > 0
        /// If a product is not linked, its LinkId == null or < 0 (LinkId may be -Id to differ from all other LinkId's);
        /// </summary>
        /// <param name="dbc"></param>
        /// <returns></returns>
        static int get_minimal_free_link_id(Cliver.Bot.DbConnection dbc)
        {
            int? link_id = (int?)dbc[@"SELECT MIN(a.LinkId + 1)
FROM (SELECT LinkId FROM Products WHERE LinkId>0) a LEFT OUTER JOIN (SELECT LinkId FROM Products WHERE LinkId>0) b ON (a.LinkId + 1 = b.LinkId)
WHERE b.LinkId IS NULL"].GetSingleValue();
            if (link_id == null)
                link_id = 1;
            return (int)link_id;
        }

        public void SaveLink(int[] product_ids)
        {
            lock (this)
            {
                product_ids = product_ids.Distinct().ToArray();

                update_category_mapping(product_ids);

                int link_id = get_minimal_free_link_id(Dbc);

                //Fhr.ProductOffice.ProductOfficeDataContext db = DbApi.RenewContext();
                Dictionary<int, int> company_ids2product_id = new Dictionary<int, int>();
                foreach (int product_id in product_ids)
                {
                    Fhr.ProductOffice.Models.Product p = Db.Products.Where(r => r.Id == product_id).FirstOrDefault();
                    if (p == null)
                        continue;
                    if (company_ids2product_id.ContainsKey(p.CompanyId))
                        throw new Exception("Products with Id: " + p.Id + " and " + company_ids2product_id[p.CompanyId] + " belong to the same company: " + p.Company.Name + " and so cannot be linked");
                    company_ids2product_id[p.CompanyId] = p.Id;
                    p.LinkId = link_id;
                }
                Db.Configuration.ValidateOnSaveEnabled = false;
                Db.SaveChanges();
            }
        }

        void update_category_mapping(int[] product_ids)
        {
            foreach(int pi in product_ids)
            {
                Product product1 = Products.Get(pi);
                var p1_s = Db.Products.Where(p => p.Category==product1.DbProduct.Category && p.Id!=pi);
                var p2s = (from x in Db.Products.Where(p => p.Id == pi) join y in Db.Products on x.LinkId equals y.LinkId where !product_ids.Contains(y.Id) select y).ToList();
                foreach (Fhr.ProductOffice.Models.Product p2 in p2s)
                {
                    if (null == (from x in p1_s join y in Db.Products.Where(p => p.Category == p2.Category && p.Id != p2.Id) on x.LinkId equals y.LinkId select y).FirstOrDefault())
                        //remove mapping 
                        Configuration.UnmapCategoriesAndSave(product1.DbProduct, p2);
                }                
            }

            for (int i = 0; i < product_ids.Length; i++)
                for (int j = i + 1; i < product_ids.Length; i++)
                    Configuration.MapCategoriesAndSave(Products.Get(product_ids[i]), Products.Get(product_ids[j]));
        }

        #region API for self-training
        //!!!Update Configuration in Engine being kept for reuse, after performing self-training!!!
        public void PerformSelfTraining()
        {
            lock (this)
            {
                Configuration.PrepareForSelfTraining();
                Dictionary<int?, List<Fhr.ProductOffice.Models.Product>> link_ids2linked_products = Db.Products.GroupBy(p => p.LinkId).Where(g => g.Key > 0 && g.Count() > 1).ToDictionary(g => g.Key, g => g.ToList());
                foreach (List<Fhr.ProductOffice.Models.Product> lps in link_ids2linked_products.Values)
                    analyse_link(lps.Select(p => p.Id).ToArray());
                Configuration.SaveAfterSelfTraining();
            }
        }

        void analyse_link(int[] linked_product_ids)
        {
            lock (this)
            {
                for (int i = 0; i < linked_product_ids.Length; i++)
                    for (int j = i + 1; j < linked_product_ids.Length; j++)
                    {
                        int product1_id = linked_product_ids[i];
                        int product2_id = linked_product_ids[j];
                        Product product1 = Products.Get(product1_id);
                        Product product2 = Products.Get(product2_id);

                        Configuration.MapCategories(product1, product2);

                        //Dictionary<Field, HashSet<string>> matched_words = new Dictionary<Field, HashSet<string>>();
                        ////matched_words[Field.Category] = new HashSet<string>();
                        ////foreach (string word in product1.Words(Field.Category))
                        ////    if (product2.Words2Count(Field.Category).ContainsKey(word))
                        ////        matched_words[Field.Category].Add(word);
                        //matched_words[Field.Name] = new HashSet<string>();
                        //foreach (string word in product1.Words(Field.Name))
                        //    if (product2.Words2Count(Field.Name).ContainsKey(word))
                        //        matched_words[Field.Name].Add(word);

                        //List<ProductLink> pls = create_identical_Product_list_for_training(product1_id, product2.DbProduct.CompanyId);
                        //foreach (ProductLink pl in pls)
                        //{
                        //    if (null != pl.Product2s.Where(x => x.DbProduct.Id == product2_id).FirstOrDefault())
                        //        break;
                        //    foreach (Product p2 in pl.Product2s)
                        //    {
                        //        if (product1_id == p2.DbProduct.Id)
                        //            continue;
                        //        Dictionary<Field, List<string>> mws = pl.Get(product1_id, p2.DbProduct.Id).MatchedWords;
                        //        //List<string> week_mws = mws[Field.Category].Where(x => !matched_words[Field.Category].Contains(x)).ToList();
                        //        //foreach (string word in week_mws)
                        //        //{
                        //        //    Configuration.Get(product1).SetWordWeight(word, 0.9 * Configuration.Get(product1).GetWordWeight(word));
                        //        //    Configuration.Get(product2).SetWordWeight(word, 0.9 * Configuration.Get(product2).GetWordWeight(word));
                        //        //}
                        //        List<string> week_mws = mws[Field.Name].Where(x => !matched_words[Field.Name].Contains(x)).ToList();
                        //        foreach (string word in week_mws)
                        //        {
                        //            Configuration.Company c1 = Configuration.Get(product1);
                        //            c1.SetWordWeight(word, 0.9 * c1.GetWordWeight(word));
                        //            Configuration.Company c2 = Configuration.Get(product2);
                        //            c2.SetWordWeight(word, 0.9 * c2.GetWordWeight(word));
                        //        }
                        //    }
                        //}
                    }
            }
        }

        //List<ProductLink> create_identical_Product_list_for_training(int product1_id, int company2_id)
        //{
        //    List<ProductLink> pls = (from x in Companies.Get(company2_id).DbCompany.Products select new ProductLink(this, Products.Get(product1_id), Products.Get(x.Id))).ToList();
        //    pls = pls.OrderByDescending(x => x.Score).OrderByDescending(x => x.SecondaryScore).ToList();
        //    return pls;
        //}
        #endregion

        #region API for analysing data
        //!!!Update Configuration in Engine being kept for reuse, after performing self-training!!!
        public void PerformDataAnalysis(int company_id)
        {
            lock (this)
            {
                Configuration.Company cc = Configuration.Get(company_id);
                cc.PrepareForDataAnalysis();
                foreach (string w in Companies.Get(company_id).Words2ProductIds(Field.Name).Keys)
                    cc.DefineWordWeight(w);
                cc.SaveAfterDataAnalysis();
            }
        }
        
        #endregion
    }
}