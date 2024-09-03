using System;
using System.Collections.Generic;

namespace KFD
{
    class Currency
    {
        public static readonly Currency RUB = new Currency(1, "RUB");
        public static readonly Currency USD = new Currency(2, "USD");
        public static readonly Currency USDT = new Currency(3, "USDT");
        public static readonly Currency EUR = new Currency(4, "EUR");
        public static readonly Currency BTC = new Currency(5, "BTC");

        public string Name { get; set; }
        public int Type { get; set; }

        public Currency(int type, string name)
        {
            Type = type;
            Name = name;
        }
        public Currency(int type)
        {
            this.Type = type;
            if (type == Currency.RUB.Type)
                this.Name = "RUB";
            else if (type == Currency.USD.Type)
                this.Name = "USD";
            else if (type == Currency.USDT.Type)
                this.Name = "USDT";
            else if (type == Currency.EUR.Type)
                this.Name = "EUR";
            else if (type == Currency.BTC.Type)
                this.Name = "BTC";
            else
                throw new Exception("К такому жизнь нас не готовила)");
        }

        static public int GetTypeByName(string name)
        {
            name = name.Trim(); // удалим пробелы в начале и в конце имени
            switch (name)
            {
                case "RUB": return Currency.RUB.Type;
                case "USD": return Currency.USD.Type;
                case "USDT": return Currency.USDT.Type;
                case "EUR": return Currency.EUR.Type;
                case "BTC": return Currency.BTC.Type;
                default: throw new Exception("Неизвестная валюта \"" + name + "\"");
            }
        }

        public override string ToString() { return this.Name; }

        public static bool operator ==(Currency a, Currency b)
        {
            // если оба null или оба ссылкаются на один объект, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // если один из операндов null, но не оба сразу, return false.
            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            // return true, только если типы равны, иначе - false
            return a.Type == b.Type;
        }
        public static bool operator !=(Currency a, Currency b)
        {
            // если оба null или оба ссылкаются на один объект, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // если один из операндов null, но не оба сразу, return false.
            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            // return true, только если типы равны, иначе - false
            return a.Type != b.Type;
        }
    }

    class Money
    {
        public Currency Currency { get; set; }
        public double Balance { get; set; }

        public Money(Currency type, double balance = 0)
        {
            Currency = type;
            Balance = balance;
        }

        public override string ToString() { return this.Balance.ToString("0." + new string('#', 339)) + " " + this.Currency.Name; }
    }

    class CurrencyPair
    {
        public Currency To { get; set; }
        public Currency From { get; set; }
        public double ExchangeRate { get; set; }

        public CurrencyPair(Currency to, Currency from, double rate)
        {
            To = to;
            From = from;
            ExchangeRate = rate;
        }

        // изменить курс обмена случайный образом не более чем на 5% в боьшую или меньшую сторону
        public void ShakeExchangeRate()
        {
            this.ExchangeRate += ((new Random().NextDouble() * 2) - 1) * (this.ExchangeRate * 0.05);
        }

        public override string ToString()
        {
            return this.To.ToString() + "/" + this.From.ToString() + " = " + this.ExchangeRate.ToString("0." + new string('#', 339));
        }
    }

    class User
    {
        public string Name { get; set; }
        public Dictionary<int, Money> CashDict { get; set; }

        public User(string name)
        {
            Name = name;
            CashDict = new Dictionary<int, Money>();
            CashDict.Add(Currency.RUB.Type, new Money(Currency.RUB, 1000000)); // изначально у пользователя 1 млн руб наличных
        }
        public Money GetCash(Currency currency, double value)
        {
            Money cash;
            if (!CashDict.TryGetValue(currency.Type, out cash))
            {
                return new Money(currency, 0); // у пользователя нет этой валюты вообще
            }
            else if (cash.Balance >= value)
            {
                // если у пользователя этой валюты хватает, возвращаем трубуемую сумму и уменьшаем в кошельке остаток
                CashDict[currency.Type].Balance -= value;
                return new Money(currency, value);
            }
            else
            {
                // если требуемой валюты в кошельке не хватает, выдаём столько, сколько есть и обнуляем кошелёк
                double rest = cash.Balance;
                CashDict[currency.Type].Balance = 0;
                return new Money(currency, rest);
            }
        }
        public void PutCash(Money value)
        {
            Money cash;
            if (CashDict.TryGetValue(value.Currency.Type, out cash))
            {
                // такая валюта у пользорвателя уже есть, значит добавляем к уже имеющейся ещё
                CashDict[value.Currency.Type].Balance += value.Balance;
            }
            else
            {
                // такой валюты у пользователя ещё нет, просто добавляем нужную сумму к нему в кошелёк
                CashDict.Add(value.Currency.Type, new Money(value.Currency, value.Balance));
            }
        }
        public void Print()
        {
            Console.WriteLine("Текущее содержимое кошелька пользователя \"" + this.Name + "\":");
            foreach (var p in CashDict)
            {
                Console.WriteLine(p.Value.ToString());
            }
        }
    }

    class CurrencyTerminal
    {
        public Dictionary<int, Money> CashDict { get; set; }
        public List<CurrencyPair> CPList { get; set; }

        public CurrencyTerminal()
        {
            CashDict = new Dictionary<int, Money>();
            CashDict.Add(Currency.RUB.Type, new Money(Currency.RUB, 10000)); // изначально в обменном терминале 10 тыс руб
            CashDict.Add(Currency.USD.Type, new Money(Currency.USD, 1000)); // изначально в обменном терминале 1 тыс USD
            CashDict.Add(Currency.EUR.Type, new Money(Currency.EUR, 1000)); // изначально в обменном терминале 1 тыс EUR
            CashDict.Add(Currency.USDT.Type, new Money(Currency.USDT, 1000)); // изначально в обменном терминале 1 тыс USDT
            CashDict.Add(Currency.BTC.Type, new Money(Currency.BTC, 1.5)); // изначально в обменном терминале 1.5 BNC

            // начальное значение обменных курсов валютных пар
            CPList = new List<CurrencyPair>();
            // у вас в условиях как-то не очень верно написано)))
            // у вас написано: "если 1 RUB = 90 USD, то можно купить 1 USD за 90 RUB" - напутали? - не 1 RUB = 90 USD, а наоборот 1 USD = 90 RUB?
            CPList.Add(new CurrencyPair(Currency.USD, Currency.RUB, 90.0));
            CPList.Add(new CurrencyPair(Currency.EUR, Currency.RUB, 100.0));
            CPList.Add(new CurrencyPair(Currency.USD, Currency.EUR, 0.9));
            CPList.Add(new CurrencyPair(Currency.USD, Currency.USDT, 0.99));
            CPList.Add(new CurrencyPair(Currency.USD, Currency.BTC, 0.000017));
        }

        public void Print()
        {
            Console.WriteLine("Текущее содержимое терминала:");
            foreach (var p in CashDict)
            {
                Console.WriteLine(p.Value.ToString());
            }

            Console.WriteLine(Environment.NewLine + "Обменные курсы:");
            foreach (var p in CPList)
            {
                Console.WriteLine(p.ToString());
            }
        }

        public Money GetCash(Currency currency, double value)
        {
            Money cash;
            if (!CashDict.TryGetValue(currency.Type, out cash))
            {
                throw new Exception("В терминале не такой валюты \"" + currency.Name + "\" вообще");
            }
            else if (cash.Balance >= value)
            {
                // в терминале этой валюты хватает, возвращаем трубуемую сумму и уменьшаем в кошельке остаток
                CashDict[currency.Type].Balance -= value;
                return new Money(currency, value);
            }
            else
            {
                throw new Exception("В теминале нет необходимой суммы для обмена. Требуется " + value.ToString("0." + new string('#', 339)) + " " + currency.Name + ". В терминале есть только " + cash.ToString());
            }
        }
        public void PutCash(Money value)
        {
            Money cash;
            if (CashDict.TryGetValue(value.Currency.Type, out cash))
            {
                // такая валюта у пользорвателя уже есть, значит добавляем к уже имеющейся ещё
                CashDict[value.Currency.Type].Balance += value.Balance;
            }
            else
            {
                // такой валюты у пользователя ещё нет, просто добавляем нужную сумму к нему в кошелёк
                CashDict.Add(value.Currency.Type, new Money(value.Currency, value.Balance));
            }
        }

        public Money DoExchange(User user, Currency from, Currency to, double value)
        {
            foreach (var p in CPList)
            {
                if (p.From == from && p.To == to) // нужную для обмена пару нашли
                {
                    return _DoExchange(user, from, to, p, value);
                }
                else if (p.From == to && p.To == from) // или нашли курс для обратного обмена (по условиям задачи туда и обратно по тому же курсу можно конвертировать)
                {
                    CurrencyPair pair = new CurrencyPair(to, from, 1 / p.ExchangeRate);
                    return _DoExchange(user, from, to, pair, value);
                }
            }

            throw new Exception("Не найден необходимый обменный курс для " + to.Name + "/" + from.Name);
        }
        private Money _DoExchange(User user, Currency from, Currency to, CurrencyPair p, double value)
        {
            var cash = user.GetCash(from, value); // берём из кошелька необходимую сумму для обмена
            if (cash.Balance < value)
            {
                user.PutCash(cash); // возвращаем в кошелёк взятую сумму (её не достаточно)
                throw new Exception("У пользователя \"" + user.Name + "\" недостаточно средств для операции обмена.");
            }
            else
            {
                double rate = p.ExchangeRate;
                // BTC и UDST - не округляем, остальные валюты округляем до копеек
                double dstValue = to.Type == Currency.BTC.Type || to.Type == Currency.USDT.Type ? cash.Balance / rate : Math.Round((cash.Balance / rate) * 100) / 100;
                user.PutCash(cash); // на случай, если в терминале не окажется нужной суммы и возникнет Exception, пока вернём пользователю его деньги,
                // а после этого возьмём у него их опять, если в терминале есть нужная сумма
                var dstCash = this.GetCash(to, dstValue); // пытаемся взять в терминале необходимую сумму для обмена (если такой суммы нет - будет Excaption)
                cash = user.GetCash(from, value); // в терминале нужная сумма есть, поэтому опять берём из кошелька пользователя необходимую сумму для обмена

                // если мы уже здесь - значит всё хорошо - денег у всех хватает)
                // у пользователя мы наличные в исходной валюте уже взяли и проверили, что их хватает (это cash)
                // в терминале в результируещей валюте мы тоже уже взяли наличные и проверили, что в терминале их тоже хватает) (это dstCash)

                this.PutCash(cash); // положим в терминал исходные деньги пользователя
                user.PutCash(dstCash); // положим в кошелёк пользователя деньги в результрующей валюте ему
                ShakeExchangeRates(); // перетрясём обменные курсы случайным образом после успешного обмена

                return dstCash;
            }
        }

        private void ShakeExchangeRates()
        {
            foreach (var p in CPList)
            {
                p.ShakeExchangeRate();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CurrencyTerminal terminal = new CurrencyTerminal(); // создадим экземплар терминала
            User user = new User("Иванов Иван Иванович"); // создадим одного пользователя и назовём его Иванов Иван Иванович)

            Console.WriteLine("Обменный автомат. Для выхода введите quit" + Environment.NewLine); // добавим пустую строку для "красоты"

            while (true) // бесконечный цикл ввода пользователя
            {
                try
                {
                    terminal.Print();
                    Console.WriteLine(""); // добавим пустую строку, чтобы отделить содержимое терминала от содержимого кошелька пользователя

                    user.Print();
                    Console.WriteLine(""); // добавим пустую строку для "красоты")

                    Console.WriteLine("Введите наименование валюты, которую желаете получить в результате обмена:");
                    string inputCurrencyName = Console.ReadLine().Trim();
                    if (inputCurrencyName == "quit")
                        break;
                    int currencyToType = Currency.GetTypeByName(inputCurrencyName); // если ввести несуществующую валюту, будет Exception

                    Console.WriteLine("Введите сколько у вас есть для обмена в формате \"Число пробел наименование валюты\", например, \"100 RUB\":");
                    string inputSourceMoney = Console.ReadLine().Trim();
                    if (inputSourceMoney == "quit")
                        break;
                    var parts = inputSourceMoney.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                        throw new Exception("Некорректный ввод. Необходимо ввести только сумму и валюту через пробел. Вы же ввели: \"" + inputSourceMoney + "\"");

                    double srcValue;
                    if (!double.TryParse(parts[0], out srcValue))
                    {
                        throw new Exception("Сумма введена некорректно. Это должно быть целое или дробное число (если сумма с копейками)");
                    }

                    // теперь проверим ввод второй части - валюты
                    int currencyFromType = Currency.GetTypeByName(parts[1]); // если ввести несуществующую валюту, будет Exception

                    var money = terminal.DoExchange(user, new Currency(currencyFromType), new Currency(currencyToType), srcValue);
                    Console.WriteLine(
                        "В результате обмена " + inputSourceMoney + " на " + inputCurrencyName + " пользователь \"" + user.Name +
                        "\" получил " + money.ToString() + Environment.NewLine
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + Environment.NewLine);
                }
            }
        }
    }
}
