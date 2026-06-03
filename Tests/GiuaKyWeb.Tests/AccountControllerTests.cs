using GiuaKyWeb.Controllers;
using GiuaKyWeb.Models;
using GiuaKyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GiuaKyWeb.Tests;

public class AccountControllerTests
{
    [Fact]
    public async Task Register_Post_AssignsStudentRole()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = BuildUserManager(userStore.Object);
        var signInManager = BuildSignInManager(userManager.Object);

        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable();

        var controller = new AccountController(userManager.Object, signInManager.Object);

        var result = await controller.Register(new RegisterViewModel
        {
            UserName = "student1",
            Email = "student1@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        Assert.IsType<RedirectToActionResult>(result);
        userManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"), Times.Once);
    }

    [Fact]
    public async Task Login_Post_RedirectsToHome_WhenCredentialsAreValid()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = BuildUserManager(userStore.Object);
        var signInManager = BuildSignInManager(userManager.Object);

        signInManager.Setup(x => x.PasswordSignInAsync("student1", "Password123!", false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = new AccountController(userManager.Object, signInManager.Object);

        var result = await controller.Login(new LoginViewModel
        {
            UserName = "student1",
            Password = "Password123!"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public async Task ExternalLoginCallback_CreatesStudentUser_WhenGoogleLoginIsFirstTime()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = BuildUserManager(userStore.Object);
        var signInManager = BuildSignInManager(userManager.Object);

        var principal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "googleuser@example.com"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "googleuser")
                },
                "Google"));

        var info = new ExternalLoginInfo(
            principal,
            "Google",
            "provider-key-1",
            "Google");

        signInManager.Setup(x => x.GetExternalLoginInfoAsync(null))
            .ReturnsAsync(info);
        signInManager.Setup(x => x.ExternalLoginSignInAsync("Google", "provider-key-1", false, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);
        userManager.Setup(x => x.FindByEmailAsync("googleuser@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddLoginAsync(It.IsAny<ApplicationUser>(), info))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable();

        var controller = new AccountController(userManager.Object, signInManager.Object);

        var result = await controller.ExternalLoginCallback();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        userManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        userManager.Verify(x => x.AddLoginAsync(It.IsAny<ApplicationUser>(), info), Times.Once);
        userManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"), Times.Once);
    }

    private static Mock<UserManager<ApplicationUser>> BuildUserManager(IUserStore<ApplicationUser> store)
    {
        return new Mock<UserManager<ApplicationUser>>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
    }

    private static Mock<SignInManager<ApplicationUser>> BuildSignInManager(UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        contextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        return new Mock<SignInManager<ApplicationUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            Options.Create(new IdentityOptions()),
            new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new DefaultUserConfirmation<ApplicationUser>());
    }
}
