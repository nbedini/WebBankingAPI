using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WebBankingAPI.Models;

namespace WebBankingAPI.Controllers
{
    [Route("/api/conti-correnti")]
    [ApiController]
    public class ContiCorrentiController : ControllerBase
    {

        #region Conti Correnti

        [Authorize]
        [HttpGet("")]
        public ActionResult ContiCorrenti()
        {
            var id = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var username = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                if (model.Users.FirstOrDefault(fod => fod.Id == id && fod.Username == username).IsBanker)
                    return Ok(model.BankAccounts.ToList());
                else
                {
                    return Ok(model.BankAccounts.Where(w => w.FkUser == id).Select(s => new { s.Id, s.Iban }).ToList());
                }
            }
        }

        #endregion

        #region Conto Corrente

        [Authorize]
        [HttpGet("{id}")]
        public ActionResult ContoCorrente(int id)
        {
            var idtoken = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var usernametoken = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                if(model.BankAccounts.FirstOrDefault(fod => fod.Id == id) != null)
                {
                    if (model.Users.FirstOrDefault(fod => fod.Id == idtoken && fod.Username == usernametoken).IsBanker)
                        return Ok(model.BankAccounts.FirstOrDefault(fod => fod.Id == id));
                    else
                    {
                        if (model.BankAccounts.FirstOrDefault(fod => fod.Id == id).FkUser == idtoken)
                            return Ok(model.BankAccounts.Select(s => new { s.Id, s.Iban }).FirstOrDefault(fod => fod.Id == id));
                        else
                            return Problem("Non è un tuo conto corrente");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }

        #endregion

        #region Movimenti Totali

        [Authorize]
        [HttpGet("{id}/movimenti")]
        public ActionResult MovimentiTotali(int id)
        {
            var idtoken = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var usernametoken = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                if (model.BankAccounts.FirstOrDefault(fod => fod.Id == id) != null)
                {
                    if (model.Users.FirstOrDefault(fod => fod.Id == idtoken && fod.Username == usernametoken).IsBanker)
                        return Ok(model.AccountMovements
                            .Where(w => w.FkBankAccountNavigation.Id == id)
                            .OrderBy(ob => ob.Date)
                            .Select(s => new { s.Id, s.Description, s.In, s.Out, s.Date })
                            .ToList());
                    else
                    {
                        if (model.BankAccounts.FirstOrDefault(fod => fod.Id == id).FkUser == idtoken)
                            return Ok(model.AccountMovements
                            .Where(w => w.FkBankAccountNavigation.Id == id)
                            .OrderBy(ob => ob.Date)
                            .Select(s => new { s.Id, s.Description, s.In, s.Out, s.Date })
                            .ToList());
                        else
                            return Problem("Non è un tuo conto corrente");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }

        #endregion

        #region Saldo

        [Authorize]
        [HttpGet("{id}/saldo")]
        public ActionResult Saldo(int id)
        {
            var idtoken = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var usernametoken = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                if (model.Users.FirstOrDefault(fod => fod.Id == idtoken && fod.Username == usernametoken).IsBanker)
                {
                    var bankaccount = model.BankAccounts.Include(i => i.FkUserNavigation).FirstOrDefault(fod => fod.Id == id);
                    var AccountMovement = model.AccountMovements.Where(w => w.FkBankAccountNavigation == bankaccount);
                    var candidato = new ContoCorrenteSpec(bankaccount.Iban, bankaccount.FkUserNavigation.Id, AccountMovement.Sum(s => (s.In == null ? 0 : s.In) - (s.Out == null ? 0 : s.Out)), bankaccount.Id);
                    return Ok(candidato);
                }
                else
                {
                    if(model.BankAccounts.Where(w => w.FkUserNavigation.Id == idtoken && w.Id == id).Count() > 0)
                    {
                        var bankaccount = model.BankAccounts.Include(i => i.FkUserNavigation).FirstOrDefault(fod => fod.Id == id);
                        var AccountMovement = model.AccountMovements.Where(w => w.FkBankAccountNavigation == bankaccount);
                        var candidato = new ContoCorrenteSpec(bankaccount.Iban, bankaccount.FkUserNavigation.Id, AccountMovement.Sum(s => (s.In == null ? 0 : s.In) - (s.Out == null ? 0 : s.Out)), bankaccount.Id);
                        return Ok(candidato);
                    }
                    else
                    {
                        return Problem();
                    }
                }
            }
        }

        #endregion

        #region Movimento

        [Authorize]
        [HttpGet("{id}/movimenti/{idmovimento}")]
        public ActionResult Movimento(int id, int idmovimento)
        {
            var idtoken = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var usernametoken = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                if (model.BankAccounts.FirstOrDefault(fod => fod.Id == id) != null)
                {
                    if (model.Users.FirstOrDefault(fod => fod.Id == idtoken && fod.Username == usernametoken).IsBanker && model.AccountMovements.Where(w => w.Id == idmovimento && w.FkBankAccountNavigation.Id == id).Count() > 0)
                        return Ok(model.AccountMovements
                            .Where(w => w.FkBankAccountNavigation.Id == id && w.Id == idmovimento)
                            .Select(s => new { s.Id, s.Description, s.In, s.Out, s.Date })
                            .ToList());
                    else
                    {
                        if (model.BankAccounts.FirstOrDefault(fod => fod.Id == id).FkUser == idtoken && model.AccountMovements.Where(w => w.Id == idmovimento && w.FkBankAccountNavigation.Id == id).Count() > 0)
                            return Ok(model.AccountMovements
                                .Where(w => w.FkBankAccountNavigation.Id == id && w.Id == idmovimento)
                                .Select(s => new { s.Id, s.Description, s.In, s.Out, s.Date })
                                .ToList());
                        else
                            return Problem("Non è un tuo conto corrente");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }

        #endregion

        #region Bonifico

        [Authorize]
        [HttpPost("{id}/bonifico")]
        public ActionResult Bonifico(int id, [FromBody]BonificoSpec bonificospec)
        {
            var idtoken = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var usernametoken = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                double? saldo = model.AccountMovements.Where(w => w.FkBankAccountNavigation.Id == id).Sum(s => (s.In == null ? 0 : s.In) - (s.Out == null ? 0 : s.Out));
                if (bonificospec == null || bonificospec.Importo < 0 || saldo < bonificospec.Importo)
                    return Problem();
                else
                {
                    if(model.Users.FirstOrDefault(fod => fod.Id == idtoken && fod.Username == usernametoken).IsBanker)
                    {
                        if (model.BankAccounts.Where(w => w.Iban == bonificospec.Iban).Count() > 0)
                        {
                            model.AccountMovements.Add(new AccountMovement { FkBankAccount = id, Date = DateTime.Now, Description = "Bonifico Inviato", Out = bonificospec.Importo });
                            model.AccountMovements.Add(new AccountMovement { FkBankAccount = model.BankAccounts.FirstOrDefault(fod => fod.Iban == bonificospec.Iban).Id, Date = DateTime.Now, Description = "Bonifico Ricevuto", In = bonificospec.Importo });
                            model.SaveChanges();
                            return Ok();
                        }
                        else
                        {
                            model.AccountMovements.Add(new AccountMovement { FkBankAccount = id, Date = DateTime.Now, Description = "Bonifico Inviato", Out = bonificospec.Importo });
                            model.SaveChanges();
                            return Ok();
                        }
                    }
                    else
                    {
                        if (model.BankAccounts.Where(w => w.FkUserNavigation.Id == idtoken && w.Id == id).Count() > 0)
                        {
                            if (model.BankAccounts.Where(w => w.Iban == bonificospec.Iban).Count() > 0)
                            {
                                model.AccountMovements.Add(new AccountMovement { FkBankAccount = id, Date = DateTime.Now, Description = "Bonifico Inviato", Out = bonificospec.Importo });
                                model.AccountMovements.Add(new AccountMovement { FkBankAccount = model.BankAccounts.FirstOrDefault(fod => fod.Iban == bonificospec.Iban).Id, Date = DateTime.Now, Description = "Bonifico Ricevuto", In = bonificospec.Importo });
                                model.SaveChanges();
                                return Ok();
                            }
                            else
                            {
                                model.AccountMovements.Add(new AccountMovement { FkBankAccount = id, Date = DateTime.Now, Description = "Bonifico Inviato", Out = bonificospec.Importo });
                                model.SaveChanges();
                                return Ok();
                            }
                        }
                        else
                        {
                            return Problem();
                        }
                    }
                }
            }
        }

        #endregion

        #region Create

        [Authorize]
        [HttpPost("")]
        public ActionResult Create([FromBody]ContoCorrenteSpec contocorrentespec)
        {
            var idtoken = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var usernametoken = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                if(model.Users.FirstOrDefault(fod => fod.Id == idtoken).IsBanker && model.BankAccounts.Where(w => w.Iban == contocorrentespec.Iban).Count() == 0)
                {
                    model.BankAccounts.Add(new BankAccount { Iban = contocorrentespec.Iban, FkUser = contocorrentespec.User });
                    model.SaveChanges();
                    return Ok();
                }
                else
                {
                    return Unauthorized();
                }
            }
        }

        #endregion

        #region Update

        [Authorize]
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody]ContoCorrenteSpec contocorrenteupdated)
        {
            var idtoken = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var usernametoken = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                if (model.Users.FirstOrDefault(fod => fod.Id == idtoken).IsBanker && model.BankAccounts.Where(w => w.Iban == contocorrenteupdated.Iban).Count() == 0)
                {
                    var candidate = model.BankAccounts.FirstOrDefault(fod => fod.Id == id);
                    if (contocorrenteupdated != null && candidate != null)
                    {
                        candidate.FkUser = contocorrenteupdated.User;
                        candidate.Iban = contocorrenteupdated.Iban;
                        model.SaveChanges();
                        return Ok();
                    }
                    return Problem();
                }
                else
                {
                    if (model.Users.FirstOrDefault(fod => fod.Id == idtoken).IsBanker)
                        return Problem("Iban già esistente");
                    else
                        return Unauthorized();
                }

            }
        }

        #endregion

        #region Delete

        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var idtoken = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            var usernametoken = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                if (model.Users.FirstOrDefault(fod => fod.Id == idtoken).IsBanker)
                {
                    var candidate = model.BankAccounts.FirstOrDefault(fod => fod.Id == id);
                    model.AccountMovements.RemoveRange(model.AccountMovements.Where(w => w.FkBankAccountNavigation.Id == candidate.Id));
                    model.BankAccounts.Remove(candidate);
                    model.SaveChanges();
                    return Ok();
                }
                else
                {
                    return Unauthorized();
                }
            }
        }

        #endregion
    }
}