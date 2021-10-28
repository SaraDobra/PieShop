using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PieShop.Models
{
    public class ShoppingCart
    {
        //The shoppingCart class will work with the _appDbContext, so we're passing it here via constructor injection.

        private readonly AppDbContext _appDbContext;

        public string ShoppingCartId { get; set; }
        public List<ShoppingCartItem> ShoppingCartItems { get; set; }
        private ShoppingCart(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        //The GetCart method is a very important one, and it now gets an IServiceProvider called services,
        ///that is basically going to give us access in this method to the services collection, basically the collection of services managed in the
        ///dependency injection container.
        // When a user comes to my site, I'm going to check if he or she already has an active shopping cart.
        // A shopping cart, I'm going to represent in memory using GUID, a string. And that string I'm going to be storing in the session.
        //So, when a user comes to my site, I'm going to check if there is already an active session containing a cart ID. If not, I create a new one.
        //Then I'm going to create an instance of a shopping cart passing in that cart Id.

        //Sessions allow me to store information on the server side
        //between the request and to an underlying mechanism of cookies
        //ASP.NET can this way remember basically keep state information
        //on the server about a certain active session

        //Through the IHttpContextAccessor I get access to the HttpContext that gives me 
        //access to all information about the request. And that way, I can also get access to the session.
        //Then I also ask the services collection for the AppDbContext.
        //I then check the session to see if it already has a string with the ID CartId,
        //If not, I will create a newGuid, and that will then be the cartId it has set in the session.
        //Finally, I create a new ShoppingCart, passing in that AppDbContext with the ShoppingCartId set as the cartId.
        public static ShoppingCart GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?
                .HttpContext.Session;
            var context = services.GetService<AppDbContext>();

            string cartId = session.GetString("CartId") ?? Guid.NewGuid().ToString();

            session.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }

        public void AddToCart(Pie pie, int amount)
        {
            //I'm going to check if that pie ID can be found for that search in ShoppingCart
            var shoppingCartItem =
                    _appDbContext.ShoppingCartItems.SingleOrDefault(
                        s => s.Pie.PieId == pie.PieId && s.ShoppingCartId == ShoppingCartId);

            //If the shoppingCartItem is null, meaning that pie was not in the shopping cart yet,
            //I create a new ShoppingCartItem and I set the Pie as the passed-in pie and of course the Amount as one.
            //I then add that ShoppingCartItem to the list currently managed by the _appDbcontext in its dbSet.
            if (shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    Pie = pie,
                    Amount = 1
                };

                _appDbContext.ShoppingCartItems.Add(shoppingCartItem);
            }

            //If I did already find it, then I simply increase the amount and then I call _appDbContext.SaveChanges,
            //that will allow me to add this ShoppingCartItem to the database
            else
            {
                shoppingCartItem.Amount++;
            }
            _appDbContext.SaveChanges();

        }

        public int RemoveFromCart(Pie pie)
        {
            var shoppingCartItem =
                       _appDbContext.ShoppingCartItems.SingleOrDefault(
                           s => s.Pie.PieId == pie.PieId && s.ShoppingCartId == ShoppingCartId);

            var localAmount = 0;

            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Amount > 1)
                {
                    shoppingCartItem.Amount--;
                    localAmount = shoppingCartItem.Amount;
                }
                else
                {
                    _appDbContext.ShoppingCartItems.Remove(shoppingCartItem);
                }
            }
            
            _appDbContext.SaveChanges();
            return localAmount;
        }

        public List<ShoppingCartItem> GetShoppingCartItems()
        {
            return ShoppingCartItems ??
                (ShoppingCartItems =
                    _appDbContext.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId)
                        .Include(s => s.Pie)
                        .ToList());
        }


        public void ClearCart()
        {
            var cartItems = _appDbContext
                .ShoppingCartItems
                .Where(cart => cart.ShoppingCartId == ShoppingCartId);

            _appDbContext.ShoppingCartItems.RemoveRange(cartItems);

            _appDbContext.SaveChanges();
        }

        public decimal GetShoppingCartTotal()
        {
            var total = _appDbContext.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId)
                    .Select(c => c.Pie.Price * c.Amount).Sum();
            return total;
        }
    }
}
