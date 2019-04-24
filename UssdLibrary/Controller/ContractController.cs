﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UssdLibrary.Model;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using UssdLibrary.Helpers;
namespace UssdLibrary.Controller
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ContractController
    {
        static readonly string FILE_PATH = "configuration.json";
        public Contract CurrentContract { get; set; }
        public bool IsNewContract { get; private set; } = false;
        public bool IsNewRouter { get; private set; } = false;

        [JsonProperty]
        public List<Contract> Contracts { get; private set; } //= new List<Contract>();

        public ContractController()
        {
            if (Contracts == null)
            {
                Contracts = GetContractData();
            }
        }

        public void Add(Contract itemContract)
        {
            if (itemContract == null)
            {
                throw new ArgumentNullException(nameof(itemContract));
            }

            if (Contracts.Contains(itemContract))
            {
                //TODO : Что-то сделать
            }
            else
            {
                Contracts.Add(itemContract);
                Save();
            }
        }
        public void Add(Contract[] itemsContract)
        {
            if (itemsContract == null)
            {
                throw new ArgumentNullException(nameof(itemsContract));
            }
            foreach (var item in itemsContract)
            {
                Contracts.Add(item);
            }
        }

        /// <summary>
        /// Получаем Контракт по Имени договора.
        /// Если он не найден то возвращает новый. IsNewContract = true
        /// </summary>
        /// <param name="nameContract"></param>
        /// <returns>Контракт</returns>
        public Contract GetContract(string nameContract)
        {
            if (string.IsNullOrWhiteSpace(nameContract))
            {
                throw new ArgumentException("message", nameof(nameContract));
            }

            CurrentContract = Contracts.SingleOrDefault(m => m.NameContract == nameContract);

            if (CurrentContract != null)
            {
                IsNewContract = false;
            }
            if (CurrentContract == null)
            {
                IsNewContract = true;
                CurrentContract = new Contract(nameContract);
            }
            return CurrentContract;
        }
        /// <summary>
        /// Получаем Роутер по IP адресу
        /// </summary>
        /// <param name="ip">IP адрес роутера</param>
        /// <returns>Router</returns>
        public Router GetRouter(string ip)
        {
            Router result;

            if (ip.CheckIP())
            {
                result = (from contract in Contracts
                          let routers = contract.Routers
                          let router = (from item in routers
                                        where item.IP == ip
                                        select item).SingleOrDefault()
                          where router != null
                          select router).SingleOrDefault();

                if (result != null)
                {
                    IsNewRouter = false;
                }
                else
                {
                    IsNewRouter = true;
                    result = new Router();
                }
                return result;
            }
            throw new ArgumentException("Неправильно задан IP адрес", nameof(ip));
        }

        /// <summary>
        /// Загрузить Контракты из JSON файла
        /// </summary>
        /// <returns>Список Контрактов</returns>
        private List<Contract> GetContractData()
        {
            using (FileStream fs = new FileStream(FILE_PATH, FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    if (JsonConvert.DeserializeObject<List<Contract>>(sr.ReadToEnd()) is List<Contract> contracts)
                    {
                        return contracts;
                    }
                    else
                    {
                        return new List<Contract>();
                    }
                }
            }
        }
        public void Save()
        {
            using (FileStream fs = new FileStream(FILE_PATH, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(JsonConvert.SerializeObject(Contracts));
                }
            }
        }
        /// <summary>
        /// Очистка файла сохранений всех контрактов
        /// </summary>
        public void ClearContracts()
        {
            Contracts = default;
            Save();
        }
    }
}