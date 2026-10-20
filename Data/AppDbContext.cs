using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Data;
using System.Text.Json;

namespace ERP.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<ShippingArea> ShippingAreas => Set<ShippingArea>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Msfield> Msfields => Set<Msfield>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<CustomerGroups> CustomerGroups => Set<CustomerGroups>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesForce> SalesForces => Set<SalesForce>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();
    public DbSet<ShippingAddress> ShippingAddresses => Set<ShippingAddress>();
    public DbSet<GoodsPurchaseOrder> GoodsPurchaseOrders => Set<GoodsPurchaseOrder>();
    public DbSet<GoodsPurchaseOrderItem> GoodsPurchaseOrderItems => Set<GoodsPurchaseOrderItem>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();
    public DbSet<GoodsRfq> GoodsRfqs => Set<GoodsRfq>();
    public DbSet<GoodsRfqItem> GoodsRfqItems => Set<GoodsRfqItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Role)
                    .WithMany()
                    .HasForeignKey(x => x.RoleId)
                    .IsRequired(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasIndex(x => x.ParentId);

            entity.Property(x => x.UrlSlug)
                .HasColumnName("url");

            entity.Property(x => x.SortOrder)
                .HasColumnName("sort_order");

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasIndex(x => x.ParentId);

            entity.Property(x => x.UrlSlug)
                .HasColumnName("url");

            entity.Property(x => x.SortOrder)
                .HasColumnName("sort_order");

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uuid)
                .HasColumnName("uid")
                .HasMaxLength(36);

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(255);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at");
        });

        modelBuilder.Entity<GoodsPurchaseOrder>(entity =>
        {
            entity.HasIndex(x => x.SupplierId);
            entity.HasIndex(x => x.PRId);
            entity.HasIndex(x => x.WarehouseId);;

            entity.Property(x => x.Status)
                .HasDefaultValue("draft");

            entity.Property(x => x.PODate)
                .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<ShippingArea>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("brands");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(36);

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Image)
                .HasColumnName("image")
                .HasMaxLength(255);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasIndex(x => x.Uid)
                .IsUnique();
        });

        modelBuilder.Entity<Msfield>(entity =>
        {
            entity.ToTable("msfields");

            entity.HasKey(x => x.id);

            entity.Property(x => x.id)
                .HasColumnName("id");

            entity.Property(x => x.uid)
                .HasColumnName("uid")
                .HasMaxLength(36);

            entity.Property(x => x.field_key)
                .HasColumnName("field_key")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.field_text)
                .HasColumnName("field_text")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.filed_value)
                .HasColumnName("filed_value")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.description)
                .HasColumnName("description");

            entity.Property(x => x.is_active)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.created_at)
                .HasColumnName("created_at");

            entity.Property(x => x.updated_at)
                .HasColumnName("updated_at");

            entity.Property(x => x.deleted_at)
                .HasColumnName("deleted_at");

            entity.Property(x => x.user_id)
                .HasColumnName("user_id");

            entity.HasIndex(x => x.uid)
                .IsUnique();
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("product_categories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.ParentId)
                .HasColumnName("parent_id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(36);

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(30);

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne<ProductCategory>()
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // PRODUCT ATTRIBUTE
        // =========================

        modelBuilder.Entity<ProductAttribute>(entity =>
        {
            entity.ToTable("product_attributes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("Uid")
                .HasMaxLength(36);

            entity.Property(x => x.Code)
                .HasColumnName("Code")
                .HasMaxLength(30);

            entity.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasColumnName("IsActive")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("DeletedAt");
        });

        // =========================
        // PRODUCT VARIANT
        // =========================

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("product_variants");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(36);

            entity.Property(x => x.AttributeId)
                .HasColumnName("attribute_id")
                .IsRequired();

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.DefHeight)
                .HasColumnName("def_height")
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.DefWidth)
                .HasColumnName("def_width")
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.DefLength)
                .HasColumnName("def_length")
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            entity.Property(x => x.SortOrder)
                .HasColumnName("sort_order")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at");

            // Relasi Variant → Attribute
            entity.HasOne<ProductAttribute>()
                .WithMany()
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.ProductNumber).IsUnique();
            entity.HasIndex(x => x.Sku).IsUnique();

            entity.HasIndex(x => x.CategoryId);
            entity.HasIndex(x => x.SubCategoryId);
            entity.HasIndex(x => x.BrandId);
            //entity.HasIndex(x => x.ParentId);
            entity.HasIndex(x => x.UnitId);

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.ProductNumber)
                .HasColumnName("product_number")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.Barcode)
                .HasColumnName("barcode")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.InternalCode)
                .HasColumnName("internal_code")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.ExternalCode1)
                .HasColumnName("external_code1")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.ExternalCode2)
                .HasColumnName("external_code2")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.ExternalCode3)
                .HasColumnName("external_code3")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.DimMode)
                .HasColumnName("dim_mode")
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.PriceMode)
                .HasColumnName("price_mode")
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasColumnName("description")
                .IsUnicode(false)
                .IsRequired(false);

            entity.Property(x => x.Image)
                .HasColumnName("image")
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired(false);

             
            // REQUIRED STRING PROPERTIES  
            entity.Property(x => x.Sku)
                .HasColumnName("sku")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(x => x.ProductType)
                .HasColumnName("product_type")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

        });

        //PRODUCT UNIT
        modelBuilder.Entity<ProductUnit>(entity =>
        {
            entity.ToTable("product_units");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id");

            entity.Property(x => x.UnitId)
                .HasColumnName("unit_id");

            entity.Property(x => x.Conversion)
                .HasColumnName("conversion")
                .HasColumnType("decimal(18,4)");
        });

        //WAREHOUSE
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouses");

            entity.HasKey(x => x.id);

            entity.Property(x => x.id)
                .HasColumnName("id");

            entity.Property(x => x.uid)
                .HasColumnName("uid")
                .HasMaxLength(36);

            entity.Property(x => x.code)
                .HasColumnName("code")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.address)
                .HasColumnName("address")
                .HasColumnType("text");

            entity.Property(x => x.phone)
                .HasColumnName("phone")
                .HasMaxLength(30);

            entity.Property(x => x.pic_name)
                .HasColumnName("pic_name")
                .HasMaxLength(100);

            entity.Property(x => x.is_default)
                .HasColumnName("is_default")
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(x => x.is_active)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(x => x.created_at)
                .HasColumnName("created_at");

            entity.Property(x => x.updated_at)
                .HasColumnName("updated_at");

            entity.Property(x => x.deleted_at)
                .HasColumnName("deleted_at");

            entity.HasIndex(x => x.uid)
                .IsUnique();

            entity.HasIndex(x => x.code)
                .IsUnique();
        });

        //Customer Groups 
        modelBuilder.Entity<CustomerGroups>(entity =>
        {
            entity.ToTable("customer_groups");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(40);

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.DiscountPercent)
                .HasColumnName("discount_percent")
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(255);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at");

            entity.Property(x => x.PriceListId)
                .HasColumnName("price_list_id");

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.HasIndex(x => x.Uid)
                .IsUnique();

            entity.HasIndex(x => x.PriceListId);
        });

        //customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(36)
                .IsUnicode(false);

            entity.Property(x => x.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(x => x.Password)
                .HasColumnName("password")
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasMaxLength(13)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(x => x.Npwp)
                .HasColumnName("npwp")
                .HasMaxLength(16)
                .IsUnicode(false);

            entity.Property(x => x.Nik)
                .HasColumnName("nik")
                .HasMaxLength(16)
                .IsUnicode(false);

            entity.Property(x => x.Address)
                .HasColumnName("address")
                .IsUnicode(false)
                .IsRequired();

            entity.Property(x => x.Image)
                .HasColumnName("image")
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(x => x.CustomerGroupId)
                .HasColumnName("customer_group_id");

            entity.Property(x => x.SalesForceId)
                .HasColumnName("sales_force_id");

            entity.Property(x => x.EmailVerifiedAt)
                .HasColumnName("email_verified_at");

            entity.Property(x => x.RememberToken)
                .HasColumnName("remember_token")
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.Property(x => x.RegisteredOnline)
                .HasColumnName("registered_online");

            entity.Property(x => x.PriceListId)
                .HasColumnName("price_list_id");

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at");

            entity.Property(x => x.UserId)
                .HasColumnName("user_id");

            entity.Property(x => x.ShopStatus)
                .HasColumnName("shop_status")
                .HasMaxLength(20)
                .IsUnicode(false);

            // Customer → Customer Group
            entity.HasOne(x => x.CustomerGroup)
                .WithMany()
                .HasForeignKey(x => x.CustomerGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer → Sales Force
            entity.HasOne(x => x.SalesForce)
                .WithMany()
                .HasForeignKey(x => x.SalesForceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer → Price List
            entity.HasOne(x => x.PriceList)
                .WithMany()
                .HasForeignKey(x => x.PriceListId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Uid)
                .IsUnique();
        });

        //shipping-area
        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.ToTable("shipping_addresses");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(40)
                .IsUnicode(false);

            entity.Property(x => x.CustomerId)
                .HasColumnName("customer_id")
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(x => x.Address)
                .HasColumnName("address")
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(x => x.ShippingAreaId)
                .HasColumnName("shipping_area_id");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            // ShippingAddress → Customer
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.ShippingAddresses)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ShippingAddress → Shipping Area
            entity.HasOne(x => x.ShippingArea)
                .WithMany()
                .HasForeignKey(x => x.ShippingAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        //price-list
        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.ToTable("price_lists");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(40);

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Note)
                .HasColumnName("note")
                .HasMaxLength(255);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasIndex(x => x.Code)
                .IsUnique();
        });

        //price-list-items
        modelBuilder.Entity<PriceListItem>(entity =>
        {
            entity.ToTable("price_list_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(40);

            entity.Property(x => x.PriceListId)
                .HasColumnName("price_list_id")
                .IsRequired();

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            entity.Property(x => x.UnitId)
                .HasColumnName("unit_id");

            entity.Property(x => x.Price)
                .HasColumnName("price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(x => x.TaxInclusive)
                .HasColumnName("tax_inclusive");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");


            // Relasi ke Price List
            entity.HasOne<PriceList>()
                .WithMany()
                .HasForeignKey(x => x.PriceListId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Relasi ke Product
            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            // Index
            entity.HasIndex(x => x.PriceListId); 
            entity.HasIndex(x => x.ProductId); 
            entity.HasIndex(x => new
            {
                x.PriceListId,
                x.ProductId
            })
            .IsUnique();
        });

        modelBuilder.Entity<SalesForce>(entity =>
        {
            entity.ToTable("sales_forces");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                        .HasColumnName("uid")
                        .HasMaxLength(40);

            entity.Property(x => x.Code)
                        .HasColumnName("code")
                        .HasMaxLength(20)
                        .IsRequired();

            entity.Property(x => x.Name)
                        .HasColumnName("name")
                        .HasMaxLength(100)
                        .IsRequired();

            entity.Property(x => x.Phone)
                        .HasColumnName("phone")
                        .HasMaxLength(30);

            entity.Property(x => x.Email)
                        .HasColumnName("email")
                        .HasMaxLength(100);

            entity.Property(x => x.Area)
                        .HasColumnName("area")
                        .HasMaxLength(100);

            entity.Property(x => x.IsActive)
                        .HasColumnName("is_active")
                        .IsRequired();

            entity.Property(x => x.CreatedAt)
                        .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                        .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                        .HasColumnName("deleted_at");

            entity.HasIndex(x => x.Code)
                        .IsUnique();
        });

        modelBuilder.Entity<WorkCenter>(entity =>
        {
            entity.ToTable("work_centers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(36);

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CostPerHour)
                .HasColumnName("cost_per_hour")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasIndex(x => x.Code)
                .IsUnique();
        });

        modelBuilder.Entity<GoodsPurchaseOrder>(entity =>
        {
            entity.ToTable("goods_purchase_orders");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("bigint unsigned")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasColumnType("char(36)");

            entity.Property(x => x.PONo)
                .HasColumnName("po_no")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PODate)
                .HasColumnName("po_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.SupplierId)
                .HasColumnName("supplier_id")
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.Property(x => x.PRId)
                .HasColumnName("pr_id")
                .HasColumnType("bigint unsigned");

            entity.Property(x => x.RFQId)
                .HasColumnName("rfq_id")
                .HasColumnType("bigint unsigned");

            entity.Property(x => x.WarehouseId)
                .HasColumnName("warehouse_id")
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.Property(x => x.ExpectedDate)
                .HasColumnName("expected_date")
                .HasColumnType("date");

            entity.Property(x => x.Note)
                .HasColumnName("note")
                .HasColumnType("text");

            entity.Property(x => x.SubtotalAmount)
                .HasColumnName("subtotal_amount")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.DiscountAmount)
                .HasColumnName("discount_amount")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.TaxPercent)
                .HasColumnName("tax_percent")
                .HasPrecision(5, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.TaxAmount)
                .HasColumnName("tax_amount")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType(
                    "enum('draft','pending_approval','ordered','partial','received','cancelled')")
                .HasDefaultValue("draft")
                .IsRequired();

            entity.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasColumnType("int unsigned");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<GoodsPurchaseOrderItem>(entity =>
        {
            entity.ToTable("goods_purchase_order_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("bigint unsigned")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasColumnType("char(36)");

            entity.Property(x => x.POId)
                .HasColumnName("po_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.POId)
                .IsUnique();

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DimLength)
                .HasColumnName("dim_length")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.DimWidth)
                .HasColumnName("dim_width")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.DimHeight)
                .HasColumnName("dim_height")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.DimCount)
               .HasColumnName("dim_count")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m);

            entity.Property(x => x.UnitId)
               .HasColumnName("unit_id")
               .HasMaxLength(50);

            entity.Property(x => x.UnitQty)
               .HasColumnName("unit_qty")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m);

            entity.Property(x => x.Qty)
               .HasColumnName("quantity")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m)
               .IsRequired();

            entity.Property(x => x.ReceivedQty)
               .HasColumnName("received_qty")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m)
               .IsRequired();

            entity.Property(x => x.UnitPrice)
               .HasColumnName("unit_price")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m)
               .IsRequired();

            entity.Property(x => x.DiscPercent)
               .HasColumnName("disc_percent")
               .HasPrecision(5, 2)
               .HasDefaultValue(0m);

            entity.Property(x => x.TaxPercent)
               .HasColumnName("tax_percent")
               .HasPrecision(5, 2)
               .HasDefaultValue(0m);

            entity.Property(x => x.Subtotal)
               .HasColumnName("subtotal")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m)
               .IsRequired();

        });

        modelBuilder.Entity<GoodsReceipt>(entity =>
        {
            entity.ToTable("goods_receipts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("bigint unsigned")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasColumnType("char(36)");

            entity.Property(x => x.ReceiptNo)
                .HasColumnName("receipt_no")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ReceiptDate)
                .HasColumnName("receipt_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.WarehouseId)
                .HasColumnName("warehouse_id")
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.Property(x => x.SupplierId)
                .HasColumnName("supplier_id")
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.Property(x => x.POId)
                .HasColumnName("po_id")
                .HasColumnType("bigint unsigned");

            entity.Property(x => x.RefNo)
                .HasColumnName("ref_no")
                .HasColumnType("bigint unsigned");

            entity.Property(x => x.Note)
                .HasColumnName("note")
                .HasColumnType("bigint unsigned")
                .IsRequired();

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType(
                    "enum('draft','posted','cancelled')")
                .HasDefaultValue("draft")
                .IsRequired();
       
            entity.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasColumnType("int unsigned");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<GoodsReceiptItem>(entity =>
        {
            entity.ToTable("goods_receipt_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("bigint unsigned")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasColumnType("char(36)");

            entity.Property(x => x.ReceiptId)
                .HasColumnName("receipt_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DimLength)
                .HasColumnName("dim_length")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.DimWidth)
                .HasColumnName("dim_width")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.DimHeight)
                .HasColumnName("dim_height")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(x => x.DimCount)
               .HasColumnName("dim_count")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m);

            entity.Property(x => x.UnitId)
               .HasColumnName("unit_id")
               .HasMaxLength(50);

            entity.Property(x => x.UnitQty)
               .HasColumnName("unit_qty")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m);

            entity.Property(x => x.Qty)
               .HasColumnName("quantity")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m)
               .IsRequired();

            entity.Property(x => x.UnitCost)
               .HasColumnName("unit_cost")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m)
               .IsRequired();

            entity.Property(x => x.Subtotal)
               .HasColumnName("subtotal")
               .HasPrecision(18, 2)
               .HasDefaultValue(0m)
               .IsRequired();

        });

        modelBuilder.Entity<GoodsRfq>(entity =>
        {
            entity.ToTable("goods_rfqs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Uid)
                .HasColumnName("uid")
                .HasMaxLength(36);

            entity.Property(x => x.RfqNo)
                .HasColumnName("rfq_no")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.RfqDate)
                .HasColumnName("rfq_date")
                .IsRequired();

            entity.Property(x => x.ValidUntil)
                .HasColumnName("valid_until");

            entity.Property(x => x.SupplierId)
                .HasColumnName("supplier_id")
                .IsRequired();

            entity.Property(x => x.PrId)
                .HasColumnName("pr_id");

            entity.Property(x => x.Note)
                .HasColumnName("note");

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .HasDefaultValue("draft")
                .IsRequired();

            entity.Property(x => x.SubtotalAmount)
                .HasColumnName("subtotal_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.CreatedBy)
                .HasColumnName("created_by");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasIndex(x => x.RfqNo)
                .IsUnique();
        });

        modelBuilder.Entity<GoodsRfqItem>(entity =>
        {
            entity.ToTable("goods_rfq_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RfqId).HasColumnName("rfq_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.UnitId).HasColumnName("unit_id");

            entity.Property(x => x.UnitQty)
                .HasColumnName("unit_qty")
                .HasColumnType("decimal(18,3)");

            entity.Property(x => x.DimLength)
                .HasColumnName("dim_length")
                .HasColumnType("decimal(12,3)");

            entity.Property(x => x.DimWidth)
                .HasColumnName("dim_width")
                .HasColumnType("decimal(12,3)");

            entity.Property(x => x.DimHeight)
                .HasColumnName("dim_height")
                .HasColumnType("decimal(12,3)");

            entity.Property(x => x.DimCount)
                .HasColumnName("dim_count")
                .HasColumnType("decimal(12,3)");

            entity.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .HasColumnType("decimal(18,3)");

            entity.Property(x => x.EstPrice)
                .HasColumnName("est_price")
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.QuotedPrice)
                .HasColumnName("quoted_price")
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Note)
                .HasColumnName("note")
                .HasMaxLength(255);
        });
    }
}
