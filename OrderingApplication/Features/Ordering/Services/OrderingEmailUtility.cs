using System.Text;
using OrderingDomain.Orders;

namespace OrderingApplication.Features.Ordering.Services;

internal static class OrderingEmailUtility
{
    internal static string GenerateOrderPlacedEmail( Order order )
    {
        var emailHtml = new StringBuilder();

        const string top =
            """
            <html>
                <head>
                    <style>
                        body { font-family: Arial, sans-serif; }
                        .order-details { width: 100%; border-collapse: collapse; margin: 20px 0; }
                        .order-details th, .order-details td { border: 1px solid #dddddd; text-align: left; padding: 8px; }
                        .order-details th { background-color: #f2f2f2; }
                        .order-header { font-size: 18px; font-weight: bold; margin-bottom: 10px; }
                        .order-section { margin-bottom: 20px; }
                    </style>
                </head>
            """;

        emailHtml.AppendLine( top );
        emailHtml.AppendLine(
            $"""
             <body>
                 <div class='order-header'>Order Confirmation</div>
                 <div class='order-section'>Order ID: {order.Id}</div>
                 <div class='order-section'>Date Placed: {order.DatePlaced}</div>
                 <div class='order-section'>SubTotal: {order.SubTotal}</div>
                 <div class='order-section'>SubTotal: {order.ShippingCost}</div>
                 <div class='order-section'>Shipping Cost: {order.TotalDiscount}</div>
                 <div class='order-section'>Total Discount: {order.TaxAmount}</div>
                 <div class='order-section'>Total Price: {order.TotalPrice}</div>
                 <div class='order-section'>Total Quantity: {order.TotalQuantity}</div>
                 <div class='order-section'>Order Status: {order.State}</div>
                 <div class='order-header'>Order Status</div>
                 <div class='order-section'>Status: {order.State}</div>
                 <div class='order-header'>Contact Information</div>
                 <div class='order-section'>Name: {order.CustomerName}</div>
                 <div class='order-section'>Email: {order.CustomerEmail}</div>
                 <div class='order-section'>Phone: {order.CustomerPhone}</div>
                 <div class='order-header'>Shipping Address</div>
                 <div class='order-section'>{order.OrderAddress.BillingAddressName}</div>
                 <div class='order-section'>{order.OrderAddress.BillingPosX}</div>
                 <div class='order-section'>{order.OrderAddress.BillingPosY}</div>
                 <div class='order-header'>Billing Address</div>
                 <div class='order-section'>{order.OrderAddress.ShippingAddressName}</div>
                 <div class='order-section'>{order.OrderAddress.ShippingPosX}</div>
                 <div class='order-section'>{order.OrderAddress.ShippingPosY}</div>
                 <div class='order-header'>Order Details</div>
             """ );

        foreach (var group in order.OrderGroups)
        {
            emailHtml.AppendLine(
                $"""
                 <div class='order-header'>Shipping Group</div>
                 <div class='order-section'>Id: {group.Id}</div>
                 <div class='order-section'>Id: {group.State}</div>
                 """);
            

            foreach ( var line in group.OrderLines )
            {
                emailHtml.AppendLine(
                    $"""
                     <table class='order-details'>
                         <tr>
                             <th>Unit ID</th>
                             <th>Unit Price</th>
                             <th>Quantity</th>
                             <th>Discount</th>
                             <th>Tax</th>
                         </tr>
                     <tr>
                     <tr>
                         <td>{line.Id}</td>
                         <td>{line.UnitId}</td>
                         <td>{line.UnitPrice:C}</td>
                         <td>{line.UnitDiscount:C}</td>
                         <td>{line.ShippingCost:C}</td>
                         <td>{line.Quantity}</td>
                     </tr>
                     """ );
            }
        }

        emailHtml.AppendLine(
            """
                    </table>
                </body>
            </html>
            """ );

        return emailHtml.ToString();
    }
    internal static string GenerateOrderGroupUpdateEmail( OrderGroup group, OrderState newState )
    {
        var emailHtml = new StringBuilder();

        emailHtml.AppendLine( """
                              <html>
                              <head>
                              <style>
                              body { font-family: Arial, sans-serif; }
                              .order-details { width: 100%; border-collapse: collapse; margin: 20px 0; }
                              .order-details th, .order-details td { border: 1px solid #dddddd; text-align: left; padding: 8px; }
                              .order-details th { background-color: #f2f2f2; }
                              .order-header { font-size: 18px; font-weight: bold; margin-bottom: 10px; }
                              .order-section { margin-bottom: 20px; }
                              </style>
                              </head>
                              <body>
                              <div class='order-header'>Order Update</div>
                              <div class='order-section'>Order Group ID: {group.Id}</div>
                              <div class='order-section'>Warehouse ID: {group.WarehouseId}</div>
                              <div class='order-section'>New State: {newState}</div>
                              <div class='order-section'>Last Updated: {DateTime.Now:MMMM dd, yyyy}</div>
                              <div class='order-header'>Order Lines</div>
                              <table class='order-details'>
                              <tr>
                              <th>Order Line ID</th>
                              <th>Unit ID</th>
                              <th>Unit Price</th>
                              <th>Quantity</th>
                              <th>Discount</th>
                              <th>Tax</th>
                              </tr>
                              """ );

        foreach ( var line in group.OrderLines )
        {
            emailHtml.AppendLine( $"""
                                   <tr>
                                   <td>{line.Id}</td>
                                   <td>{line.UnitId}</td>
                                   <td>{line.UnitPrice:C}</td>
                                   <td>{line.Quantity}</td>
                                   <td>{line.UnitDiscount:C}</td>
                                   </tr>
                                   """ );
        }

        emailHtml.AppendLine( """
                              </table>
                              </body>
                              </html>
                              """ );

        return emailHtml.ToString();
    }
    internal static string GenerateOrderUpdateEmail( Order order, OrderState newState )
    {
        return @"
            <html>
            <head>
            <style>
            body { font-family: Arial, sans-serif; }
            .order-header { font-size: 18px; font-weight: bold; margin-bottom: 10px; }
            .order-section { margin-bottom: 20px; }
            </style>
            </head>
            <body>
            <div class='order-header'>Order Status Update</div>
            <div class='order-section'>Order ID: " + order.Id + @"</div>
            <div class='order-section'>New Order Status: " + newState + @"</div>
            <div class='order-section'>Date: " + DateTime.Now.ToString( "MMMM dd, yyyy" ) + @"</div>
            <p>Dear " + order.CustomerName + @",</p>
            <p>We wanted to inform you that the status of your order has changed to <strong>" + newState + @"</strong>. Please find the updated details below:</p>
            <p>If you have any questions or need further assistance, feel free to contact us at support@example.com.</p>
            <p>Thank you for shopping with us!</p>
            <p>Best regards,<br />The YourShop Team</p>
            </body>
            </html>";
    }
    internal static string GenerateOrderCancelledEmail( Order order, string cancellationReason )
    {
        return @"
            <html>
            <head>
            <style>
            body { font-family: Arial, sans-serif; }
            .order-header { font-size: 18px; font-weight: bold; margin-bottom: 10px; }
            .order-section { margin-bottom: 20px; }
            </style>
            </head>
            <body>
            <div class='order-header'>Order Cancellation</div>
            <div class='order-section'>Order ID: " + order.Id + @"</div>
            <div class='order-section'>Cancellation Reason:</div>
            <p>" + cancellationReason + @"</p>
            <div class='order-section'>Date: " + DateTime.Now.ToString( "MMMM dd, yyyy" ) + @"</div>
            <p>Dear " + order.CustomerName + @",</p>
            <p>We regret to inform you that your order has been cancelled. The details are as follows:</p>
            <p>If you have any questions or need further assistance, feel free to contact us at support@example.com.</p>
            <p>Thank you for your understanding.</p>
            <p>Best regards,<br />The YourShop Team</p>
            </body>
            </html>";
    }
}