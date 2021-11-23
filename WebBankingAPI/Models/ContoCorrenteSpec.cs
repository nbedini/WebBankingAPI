namespace WebBankingAPI.Models
{
    public class ContoCorrenteSpec
    {
        public string Iban { get; set; }
        public int User { get; set; }
        public double? Saldo { get; set; }
        public long Id { get; set; }

        public ContoCorrenteSpec(string iban, int user, double? saldo, long id)
        {
            Iban = iban;
            User = user;
            Saldo = saldo;
            Id = id;
        }
    }
}
