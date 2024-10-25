using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechFixV3._0Client.OrdersServiceReference; // Ensure the service reference is correct
using TechFixV3._0Client.InventoryServiceReference; // Add reference to InventoryService

namespace TechFixV3._0Client.Supplier
{
    public partial class OrderManagement : System.Web.UI.Page
    {
        private OrdersServiceSoapClient ordersService = new OrdersServiceSoapClient();
        private InventoryServiceSoapClient inventoryService = new InventoryServiceSoapClient(); // Create an instance of InventoryService

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Get the Supplier ID from the cookies
                int supplierId = GetUserIdFromCookies();
                BindOrdersGrid(supplierId);
            }
        }

        private void BindOrdersGrid(int supplierId)
        {
            // Fetch orders specific to the supplier from the service
            var orders = ordersService.GetOrdersBySupplierId(supplierId);
            foreach (var order in orders)
            {
                // Fetch item name using the inventory service
                var inventoryItem = inventoryService.GetInventoryById(order.ItemId);
                order.ItemName = inventoryItem?.ItemName ?? "Unknown"; // Set item name
            }
            // Bind the orders to the GridView
            OrdersGridView.DataSource = orders;
            OrdersGridView.DataBind();
        }

        protected void OrdersGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "UpdateStatus")
            {
                // Retrieve the Order ID from the CommandArgument
                int orderId = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = (GridViewRow)((Button)e.CommandSource).NamingContainer;
                DropDownList statusDropDown = (DropDownList)row.FindControl("OrderStatusDropDown");

                // Get the selected status
                string newStatus = statusDropDown.SelectedValue;

                // Call the web service to update the order status
                string result = ordersService.UpdateOrder(orderId, 0, 0, 0, 0, newStatus); // You may want to pass actual values for adminId, supplierId, itemId, quantity
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('" + result + "');", true);

                // Rebind the grid to reflect the updated statuses
                int supplierId = GetUserIdFromCookies();
                BindOrdersGrid(supplierId);
            }
        }

        // Helper method to retrieve SupplierID from cookies
        private int GetUserIdFromCookies()
        {
            if (Request.Cookies["UserId"] != null)
            {
                return int.Parse(Request.Cookies["UserId"].Value);
            }

            return 0; // Default value if not found
        }
    }
}
