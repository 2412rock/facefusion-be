
using FacefusionBE.DB;
using FacefusionBE.Helpers;
using FacefusionBE.Response;
using FacefusionBE.Response.DorelAppBackend.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using OverflowBackend.Models.Requests;

namespace FacefusionBE.Services
{
    public class AuthService
    {
        private readonly FacefusionDBContext _dbContext;
        private readonly PasswordHashService _passwordHashService;
        private readonly MailService _mailService;

        public AuthService(FacefusionDBContext dbContext, PasswordHashService passwordHashService, MailService mailService)
        {
            _dbContext = dbContext;
            _passwordHashService = passwordHashService;
            _mailService = mailService;
        }

        private bool IsValidEmail(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return true;
            }

            // Check if all characters are either alphanumeric or dots and the length is within 14 characters
            bool isValid = input.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '@') && input.Length <= 30;
            var atChars = input.Count(c => c == '@');
            if (atChars > 1)
            {
                return false;
            }

            return isValid;
        }

        private async Task<string> HandleSession(string email)
        {
            var existingSession = await _dbContext.UserSessions.FirstOrDefaultAsync(s => s.Email == email && s.IsActive);
            if (existingSession != null) 
            {
                existingSession.IsActive = false;
                await _dbContext.SaveChangesAsync();
            }

            // Generate new session token
            var sessionToken = Guid.NewGuid().ToString();
            var userSession = new DBUserSession
            {
                Email = email,
                SessionToken = sessionToken,
                LastActiveTime = DateTime.UtcNow,
                IsActive = true
            };
            var existingSessions = await _dbContext.UserSessions.Where(e => e.Email == email).ToListAsync();
            for(int i = 0; i < existingSessions.Count; i++)
            {
                _dbContext.Remove(existingSessions[i]);
            }
            _dbContext.UserSessions.Add(userSession);
            await _dbContext.SaveChangesAsync();

            return sessionToken;
        }

        public async Task<Maybe<Tokens>> SignIn(string email, string password)
        {
            var maybe = new Maybe<Tokens>();
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.Email == email);

                if (user != null)
                {
                    var hashedPassword = user.Password;
                    if (_passwordHashService.VerifyPassword(password, hashedPassword))
                    {
                        var session = await HandleSession(email);
                        maybe.SetSuccess(new Tokens()
                        {
                            BearerToken = TokenHelper.GenerateJwtToken(email, session, false),
                            RefreshToken = TokenHelper.GenerateJwtToken(email, session, isRefreshToken: true, isAdmin: false),
                            Session = session
                        });
                    }
                    else
                    {
                        maybe.SetException("Invalid username or password");
                    }

                }
                else
                {
                    maybe.SetException("Invalid username or password");
                }
            }
            catch(Exception e)
            {
                maybe.SetException(e.Message);
            }
            return maybe;
        }

        public async Task<Maybe<bool>> SignUp(SignUpRequest req) 
        {
            var maybe = new Maybe<bool>(); 
            if(!string.IsNullOrEmpty(req.Email) && !IsValidEmail(req.Email))
            {
                maybe.SetException("Email invalid");
                return maybe;
            }
            var any = await _dbContext.Users.AnyAsync(element => element.Email == req.Email);
            if (!any)
            {
                var user = new DBUser()
                {
                    Password = _passwordHashService.HashPassword(req.Password),
                    Email = req.Email,
                    Credit = 60
                };
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                maybe.SetSuccess(true);
            }
            else
            {
                maybe.SetException("User already exists");
            }
            return maybe;
        }

        public async Task<Maybe<string>> RefreshToken(string token)
        {
            var email = TokenHelper.GetEmailFromToken(token);
            var result = new Maybe<string>();
            if (!TokenHelper.IsTokenExpired(token) && email != null)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.Email == email);
                if (user != null)
                {
                    var session = await HandleSession(email);
                    var refreshedToken = TokenHelper.GenerateJwtToken(email, session, isAdmin: false);
                    result.SetSuccess(refreshedToken);
                    return result;
                }

                else
                {
                    result.SetException("User does not exist");
                    return result;
                }

            }
            result.SetException("Token expired");
            return result;
        }

        public async Task<Maybe<bool>> DeleteAccount(string email)
        {
            var maybe = new Maybe<bool>();
            var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.Email == email);
            if(user != null)
            {
                _dbContext.Users.Remove(user);

                await _dbContext.SaveChangesAsync();
                maybe.SetSuccess(true);
            }
            else
            {
                maybe.SetException("User not found");
            }
            return maybe;
        }

        public async Task<Maybe<bool>> ResetPassword(string username, string oldPassword, string newPassword)
        {
            var maybe = new Maybe<bool>();

            try
            {
                if (true)
                {
                    var hashedPassword = "123";
                    if (!_passwordHashService.VerifyPassword(oldPassword, hashedPassword))
                    {
                        maybe.SetException("Invalid old password");
                    }
                    else if (oldPassword != newPassword)
                    {
                       // canReset.Item2.Password = _passwordHashService.HashPassword(newPassword);
                        //_dbContext.Update(canReset.Item2);
                        await _dbContext.SaveChangesAsync();
                        maybe.SetSuccess(true);
                    }
                    else
                    {
                        maybe.SetException("New password cant be the same as old one");
                    }
                }
            }
            catch(Exception e)
            {
                maybe.SetException(e.Message);
            }
            return maybe;
        }

        public async Task<Maybe<string>> SendVerificationCode(string email)
        {
            var maybe = new Maybe<string>();
            var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.Email == email);
            if(user != null)
            {
                if (String.IsNullOrEmpty(user.Password))
                {
                    maybe.SetException("User is google user");
                }
                else
                {
                    var verificationCode = new Random().Next(1000, 9999).ToString();
                    _mailService.SendMailToUser(verificationCode, user.Email);
                    if (VerificationCodeCollection.Values.ContainsKey(email))
                    {
                        string value;
                        VerificationCodeCollection.Values.Remove(email, out value);
                    }
                    if(VerificationCodeCollection.Values.TryAdd(email, verificationCode))
                    {
                        maybe.SetSuccess("Verification code sent");
                    }
                    else
                    {
                        maybe.SetException("Failed to generate code. Try again");
                    }
                }
            }
            else
            {
                maybe.SetException("No user found with that username");
            }
            return maybe;
        }

        public async Task<Maybe<string>> VerifyCodeAndChangePassword(string verificationCode, string email, string newPassword)
        {
            var maybe = new Maybe<string>();
            var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.Email == email);
            if (user != null)
            {
                if (String.IsNullOrEmpty(user.Password))
                {
                    maybe.SetException("User is google user");
                }
                else
                {
                    string code;
                    if (VerificationCodeCollection.Values.TryGetValue(email, out code))
                    {
                        if (code == verificationCode)
                        {
                            user.Password = _passwordHashService.HashPassword(newPassword);
                            _dbContext.Users.Update(user);
                            await _dbContext.SaveChangesAsync();
                            maybe.SetSuccess("Password changed");
                        }
                        else
                        {
                            maybe.SetException("Verification code invalid");
                        }
                    }
                    else
                    {
                        maybe.SetException("No verification code sent for this username");
                    }
                }
            }
            else
            {
                maybe.SetException("No user found with that email");
            }
            return maybe;
        }
    }
}
