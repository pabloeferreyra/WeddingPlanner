using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingPlanner.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NombreMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budget_Weddings_WeddingId",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistItem_TaskItem_TaskId",
                table: "ChecklistItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Expense_Budget_BudgetId",
                table: "Expense");

            migrationBuilder.DropForeignKey(
                name: "FK_Guest_PhoneNumber_PhoneId",
                table: "Guest");

            migrationBuilder.DropForeignKey(
                name: "FK_Guest_Weddings_WeddingId",
                table: "Guest");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AspNetUsers_UserId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Weddings_WeddingId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItem_AspNetUsers_AssignedToId",
                table: "TaskItem");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItem_Weddings_WeddingId",
                table: "TaskItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Timeline_AspNetUsers_AssignedToId",
                table: "Timeline");

            migrationBuilder.DropForeignKey(
                name: "FK_Timeline_Weddings_WeddingId",
                table: "Timeline");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_Address_AddressId",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_PhoneNumber_PhoneId",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorAssignment_Vendor_VendorId",
                table: "VendorAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorDocument_Vendor_VendorId",
                table: "VendorDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_Weddings_Address_LocationId",
                table: "Weddings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vendor",
                table: "Vendor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Timeline",
                table: "Timeline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskItem",
                table: "TaskItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notification",
                table: "Notification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guest",
                table: "Guest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Expense",
                table: "Expense");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChecklistItem",
                table: "ChecklistItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budget",
                table: "Budget");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Address",
                table: "Address");

            migrationBuilder.RenameTable(
                name: "Vendor",
                newName: "Vendors");

            migrationBuilder.RenameTable(
                name: "Timeline",
                newName: "TimeLines");

            migrationBuilder.RenameTable(
                name: "TaskItem",
                newName: "TaskItems");

            migrationBuilder.RenameTable(
                name: "Notification",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "Guest",
                newName: "Guests");

            migrationBuilder.RenameTable(
                name: "Expense",
                newName: "Expenses");

            migrationBuilder.RenameTable(
                name: "ChecklistItem",
                newName: "ChecklistItems");

            migrationBuilder.RenameTable(
                name: "Budget",
                newName: "Budgets");

            migrationBuilder.RenameTable(
                name: "Address",
                newName: "Addresses");

            migrationBuilder.RenameIndex(
                name: "IX_Vendor_PhoneId",
                table: "Vendors",
                newName: "IX_Vendors_PhoneId");

            migrationBuilder.RenameIndex(
                name: "IX_Vendor_AddressId",
                table: "Vendors",
                newName: "IX_Vendors_AddressId");

            migrationBuilder.RenameIndex(
                name: "IX_Timeline_WeddingId",
                table: "TimeLines",
                newName: "IX_TimeLines_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_Timeline_AssignedToId",
                table: "TimeLines",
                newName: "IX_TimeLines_AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItem_WeddingId",
                table: "TaskItems",
                newName: "IX_TaskItems_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItem_AssignedToId",
                table: "TaskItems",
                newName: "IX_TaskItems_AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_WeddingId",
                table: "Notifications",
                newName: "IX_Notifications_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_UserId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Guest_WeddingId",
                table: "Guests",
                newName: "IX_Guests_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_Guest_PhoneId",
                table: "Guests",
                newName: "IX_Guests_PhoneId");

            migrationBuilder.RenameIndex(
                name: "IX_Expense_BudgetId",
                table: "Expenses",
                newName: "IX_Expenses_BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_ChecklistItem_TaskId",
                table: "ChecklistItems",
                newName: "IX_ChecklistItems_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_WeddingId",
                table: "Budgets",
                newName: "IX_Budgets_WeddingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vendors",
                table: "Vendors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeLines",
                table: "TimeLines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskItems",
                table: "TaskItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guests",
                table: "Guests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Expenses",
                table: "Expenses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChecklistItems",
                table: "ChecklistItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Weddings_WeddingId",
                table: "Budgets",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistItems_TaskItems_TaskId",
                table: "ChecklistItems",
                column: "TaskId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Budgets_BudgetId",
                table: "Expenses",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guests_PhoneNumber_PhoneId",
                table: "Guests",
                column: "PhoneId",
                principalTable: "PhoneNumber",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Guests_Weddings_WeddingId",
                table: "Guests",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Weddings_WeddingId",
                table: "Notifications",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_AssignedToId",
                table: "TaskItems",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Weddings_WeddingId",
                table: "TaskItems",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLines_AspNetUsers_AssignedToId",
                table: "TimeLines",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeLines_Weddings_WeddingId",
                table: "TimeLines",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorAssignment_Vendors_VendorId",
                table: "VendorAssignment",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorDocument_Vendors_VendorId",
                table: "VendorDocument",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_Addresses_AddressId",
                table: "Vendors",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_PhoneNumber_PhoneId",
                table: "Vendors",
                column: "PhoneId",
                principalTable: "PhoneNumber",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Weddings_Addresses_LocationId",
                table: "Weddings",
                column: "LocationId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Weddings_WeddingId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistItems_TaskItems_TaskId",
                table: "ChecklistItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Budgets_BudgetId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Guests_PhoneNumber_PhoneId",
                table: "Guests");

            migrationBuilder.DropForeignKey(
                name: "FK_Guests_Weddings_WeddingId",
                table: "Guests");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Weddings_WeddingId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_AssignedToId",
                table: "TaskItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Weddings_WeddingId",
                table: "TaskItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeLines_AspNetUsers_AssignedToId",
                table: "TimeLines");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeLines_Weddings_WeddingId",
                table: "TimeLines");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorAssignment_Vendors_VendorId",
                table: "VendorAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorDocument_Vendors_VendorId",
                table: "VendorDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_Addresses_AddressId",
                table: "Vendors");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_PhoneNumber_PhoneId",
                table: "Vendors");

            migrationBuilder.DropForeignKey(
                name: "FK_Weddings_Addresses_LocationId",
                table: "Weddings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vendors",
                table: "Vendors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeLines",
                table: "TimeLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskItems",
                table: "TaskItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guests",
                table: "Guests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Expenses",
                table: "Expenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChecklistItems",
                table: "ChecklistItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.RenameTable(
                name: "Vendors",
                newName: "Vendor");

            migrationBuilder.RenameTable(
                name: "TimeLines",
                newName: "Timeline");

            migrationBuilder.RenameTable(
                name: "TaskItems",
                newName: "TaskItem");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notification");

            migrationBuilder.RenameTable(
                name: "Guests",
                newName: "Guest");

            migrationBuilder.RenameTable(
                name: "Expenses",
                newName: "Expense");

            migrationBuilder.RenameTable(
                name: "ChecklistItems",
                newName: "ChecklistItem");

            migrationBuilder.RenameTable(
                name: "Budgets",
                newName: "Budget");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "Address");

            migrationBuilder.RenameIndex(
                name: "IX_Vendors_PhoneId",
                table: "Vendor",
                newName: "IX_Vendor_PhoneId");

            migrationBuilder.RenameIndex(
                name: "IX_Vendors_AddressId",
                table: "Vendor",
                newName: "IX_Vendor_AddressId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeLines_WeddingId",
                table: "Timeline",
                newName: "IX_Timeline_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeLines_AssignedToId",
                table: "Timeline",
                newName: "IX_Timeline_AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItems_WeddingId",
                table: "TaskItem",
                newName: "IX_TaskItem_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItems_AssignedToId",
                table: "TaskItem",
                newName: "IX_TaskItem_AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_WeddingId",
                table: "Notification",
                newName: "IX_Notification_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notification",
                newName: "IX_Notification_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Guests_WeddingId",
                table: "Guest",
                newName: "IX_Guest_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_Guests_PhoneId",
                table: "Guest",
                newName: "IX_Guest_PhoneId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_BudgetId",
                table: "Expense",
                newName: "IX_Expense_BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_ChecklistItems_TaskId",
                table: "ChecklistItem",
                newName: "IX_ChecklistItem_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_WeddingId",
                table: "Budget",
                newName: "IX_Budget_WeddingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vendor",
                table: "Vendor",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Timeline",
                table: "Timeline",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskItem",
                table: "TaskItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notification",
                table: "Notification",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guest",
                table: "Guest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Expense",
                table: "Expense",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChecklistItem",
                table: "ChecklistItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budget",
                table: "Budget",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Address",
                table: "Address",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_Weddings_WeddingId",
                table: "Budget",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistItem_TaskItem_TaskId",
                table: "ChecklistItem",
                column: "TaskId",
                principalTable: "TaskItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_Budget_BudgetId",
                table: "Expense",
                column: "BudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guest_PhoneNumber_PhoneId",
                table: "Guest",
                column: "PhoneId",
                principalTable: "PhoneNumber",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Guest_Weddings_WeddingId",
                table: "Guest",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AspNetUsers_UserId",
                table: "Notification",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Weddings_WeddingId",
                table: "Notification",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItem_AspNetUsers_AssignedToId",
                table: "TaskItem",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItem_Weddings_WeddingId",
                table: "TaskItem",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Timeline_AspNetUsers_AssignedToId",
                table: "Timeline",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Timeline_Weddings_WeddingId",
                table: "Timeline",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendor_Address_AddressId",
                table: "Vendor",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendor_PhoneNumber_PhoneId",
                table: "Vendor",
                column: "PhoneId",
                principalTable: "PhoneNumber",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorAssignment_Vendor_VendorId",
                table: "VendorAssignment",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorDocument_Vendor_VendorId",
                table: "VendorDocument",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Weddings_Address_LocationId",
                table: "Weddings",
                column: "LocationId",
                principalTable: "Address",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
