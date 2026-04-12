/**
 * Main Application for Book of Receipts
 */
const app = {
  // Current state
  currentEntity: "products",
  products: [],
  currentImageIndex: 0,
  currentImages: [],
  selectedIngredients: new Set(),

  // Configuration
  config: {
    products: {
      endpoint: "/api/Products",
      title: "Продукты",
      categories: [
        "None",
        "Frozen",
        "Meat",
        "Vegetables",
        "Herbs",
        "Spices",
        "Cereals",
        "CannedFood",
        "Liquid",
        "Sweets",
      ],
      sortOptions: [
        { val: "Name", label: "Название" },
        { val: "Calories", label: "Калорийность" },
        { val: "Proteins", label: "Белки" },
        { val: "Fats", label: "Жиры" },
        { val: "Carbs", label: "Углеводы" },
        { val: "CreatedAt", label: "Дата создания" },
      ],
      columns: [
        { key: "id", label: "ID" },
        { key: "name", label: "Название" },
        {
          key: "category",
          label: "Категория",
          type: "category",
          categoryType: "ProductCategory",
        },
        { key: "caloriesPer100g", label: "Ккал/100г" },
        { key: "proteinsPer100g", label: "Белки" },
        { key: "fatsPer100g", label: "Жиры" },
        { key: "carbsPer100g", label: "Углеводы" },
        { key: "flags", label: "Флаги", type: "flags" },
      ],
      fields: [
        {
          name: "name",
          label: "Название",
          type: "text",
          required: true,
          minLength: 2,
        },
        {
          name: "category",
          label: "Категория",
          type: "select",
          options: [],
          required: true,
          categoryType: "ProductCategory",
        },
        {
          name: "photos",
          label: "Фотографии (URL)",
          type: "array",
          itemType: "url",
          itemLabel: "URL",
        },
        {
          name: "caloriesPer100g",
          label: "Ккал на 100г",
          type: "number",
          step: "0.1",
          min: 0,
          required: true,
        },
        {
          name: "proteinsPer100g",
          label: "Белки (г)",
          type: "number",
          step: "0.1",
          min: 0,
          required: true,
        },
        {
          name: "fatsPer100g",
          label: "Жиры (г)",
          type: "number",
          step: "0.1",
          min: 0,
          required: true,
        },
        {
          name: "carbsPer100g",
          label: "Углеводы (г)",
          type: "number",
          step: "0.1",
          min: 0,
          required: true,
        },
        { name: "composition", label: "Состав", type: "textarea", rows: 3 },
        {
          name: "cookingRequirement",
          label: "Требования к готовке",
          type: "select",
          options: ["ReadyToUse", "SemiFinished", "RequiresCooking"],
          required: true,
          categoryType: "CookingRequirement",
        },
        {
          name: "flags",
          label: "Флаги (через запятую: Vegan,GlutenFree)",
          type: "text",
        },
      ],
    },
    dishes: {
      endpoint: "/api/Dishes",
      title: "Блюда",
      categories: [
        "None",
        "Dessert",
        "Entree",
        "Side",
        "Drink",
        "Salad",
        "Soup",
        "Snack",
      ],
      sortOptions: [
        { val: "Name", label: "Название" },
        { val: "Calories", label: "Калорийность порции" },
        { val: "Proteins", label: "Белки" },
        { val: "Fats", label: "Жиры" },
        { val: "Carbs", label: "Углеводы" },
        { val: "Category", label: "Категория" },
      ],
      columns: [
        { key: "id", label: "ID" },
        { key: "name", label: "Название" },
        {
          key: "category",
          label: "Категория",
          type: "category",
          categoryType: "DishCategory",
        },
        { key: "caloriesPerServing", label: "Ккал/порция" },
        { key: "servingSize", label: "Вес (г)" },
        { key: "flags", label: "Флаги", type: "flags" },
      ],
      fields: [
        {
          name: "name",
          label: "Название",
          type: "text",
          required: true,
          minLength: 2,
        },
        {
          name: "category",
          label: "Категория",
          type: "select",
          options: [],
          categoryType: "DishCategory",
        },
        {
          name: "photos",
          label: "Фотографии (URL)",
          type: "array",
          itemType: "url",
          itemLabel: "URL",
        },
        {
          name: "caloriesPerServing",
          label: "Ккал на порцию",
          type: "number",
          step: "0.1",
          min: 0,
        },
        {
          name: "proteinsPerServing",
          label: "Белки (г)",
          type: "number",
          step: "0.1",
          min: 0,
        },
        {
          name: "fatsPerServing",
          label: "Жиры (г)",
          type: "number",
          step: "0.1",
          min: 0,
        },
        {
          name: "carbsPerServing",
          label: "Углеводы (г)",
          type: "number",
          step: "0.1",
          min: 0,
        },
        {
          name: "servingSize",
          label: "Размер порции (г)",
          type: "number",
          step: "1",
          min: 1,
        },
        { name: "flags", label: "Флаги (Vegan,GlutenFree)", type: "text" },
      ],
      hasIngredients: true,
    },
  },

  /**
   * Initialize application
   */
  init() {
    this.loadProductsForIngredients();
    this.switchTab("products");
  },

  /**
   * Switch between products and dishes tabs
   */
  async switchTab(entity) {
    this.currentEntity = entity;
    const config = this.config[entity];

    // Update tabs UI
    document.querySelectorAll(".tab-btn").forEach((btn) => {
      btn.classList.toggle("active", btn.dataset.tab === entity);
    });

    document.getElementById("list-title").textContent = config.title;

    // Populate filter category dropdown
    const catSelect = document.getElementById("filter-category");
    catSelect.innerHTML =
      '<option value="">Все категории</option>' +
      config.categories
        .map(
          (c) =>
            `<option value="${c}">${utils.translateEnum(c, entity === "products" ? "ProductCategory" : "DishCategory")}</option>`,
        )
        .join("");

    // Populate sort dropdown
    const sortSelect = document.getElementById("filter-sort");
    sortSelect.innerHTML = config.sortOptions
      .map((s) => `<option value="${s.val}">${s.label}</option>`)
      .join("");

    // Load data
    await this.loadData();

    // Load products for ingredient selector if switching to dishes
    if (entity === "dishes") {
      await this.loadProductsForIngredients();
    }
  },

  /**
   * Load products for ingredient selector
   */
  async loadProductsForIngredients() {
    try {
      this.products = await api.products.getAll();
      this.updateIngredientSelect();
    } catch (error) {
      console.error("Failed to load products:", error);
    }
  },

  /**
   * Update ingredient select dropdown
   */
  updateIngredientSelect() {
    const select = document.getElementById("ingredient-product-select");
    if (!select) return;

    const availableProducts = this.products.filter(
      (p) => !this.selectedIngredients.has(p.id),
    );

    select.innerHTML =
      '<option value="">-- Выберите продукт --</option>' +
      availableProducts
        .map(
          (p) => `<option value="${p.id}">${utils.escapeHtml(p.name)}</option>`,
        )
        .join("");
  },

  /**
   * Load data from API
   */
  async loadData() {
    const config = this.config[this.currentEntity];
    const tbody = document.getElementById("table-body");
    const loader = document.getElementById("loading-indicator");
    const empty = document.getElementById("empty-state");
    const table = document.getElementById("data-table");

    tbody.innerHTML = "";
    loader.classList.remove("hidden");
    empty.classList.add("hidden");
    table.classList.add("hidden");

    // Build query params
    const search = document.getElementById("filter-search").value;
    const category = document.getElementById("filter-category").value;
    const sort = document.getElementById("filter-sort").value;
    const ascending = document.getElementById("filter-order").value === "true";
    const flags = Array.from(
      document.querySelectorAll(".flag-filter:checked"),
    ).map((cb) => cb.value);

    const params = {
      search: search || undefined,
      category: category || undefined,
      sort: sort || undefined,
      ascending: ascending,
      flags: flags.length > 0 ? flags : undefined,
    };

    try {
      const data =
        this.currentEntity === "products"
          ? await api.products.getAll(params)
          : await api.dishes.getAll(params);

      table.classList.remove("hidden");

      // Render headers
      components.renderTableHeaders("table-header-row", config.columns);

      // Render body
      const actions = [
        {
          label: "👁️",
          onClick: `app.openViewModal(${this.currentEntity === "products" ? "products" : "dishes"}, event)`,
          type: "secondary",
        },
        {
          label: "✏️",
          onClick: `app.openEditModal(${this.currentEntity === "products" ? "products" : "dishes"}, event)`,
          type: "secondary",
        },
        {
          label: "🗑️",
          onClick: `app.deleteItem(${this.currentEntity === "products" ? "products" : "dishes"}, event)`,
          type: "danger",
        },
      ];

      components.renderTableBody("table-body", data, config.columns, actions);

      if (data.length === 0) {
        table.classList.add("hidden");
        empty.classList.remove("hidden");
      }

      // Store data for view/edit modals
      this.currentData = data;
    } catch (error) {
      this.showNotification(`Ошибка загрузки: ${error.message}`, "error");
      empty.textContent = `Ошибка: ${error.message}`;
      empty.classList.remove("hidden");
    } finally {
      loader.classList.add("hidden");
    }
  },

  /**
   * Open create modal
   */
  openCreateModal() {
    const config = this.config[this.currentEntity];
    document.getElementById("modal-title").textContent =
      `Добавить ${config.title.slice(0, -1)}`;
    document.getElementById("edit-id").value = "";
    document.getElementById("entity-form").reset();
    utils.clearFormErrors("entity-form");

    // Render form fields
    const fieldsConfig = JSON.parse(JSON.stringify(config.fields));
    fieldsConfig.forEach((f) => {
      if (f.type === "select" && f.options) {
        f.options =
          this.currentEntity === "products"
            ? this.config.products.categories
            : this.config.dishes.categories;
      }
    });

    components.renderFormFields("modal-fields", fieldsConfig, {}, config);

    // Show/hide ingredients section
    const ingContainer = document.getElementById("ingredients-container");
    if (config.hasIngredients) {
      ingContainer.classList.remove("hidden");
      document.getElementById("ingredients-list").innerHTML = "";
      this.selectedIngredients.clear();
      this.updateIngredientSelect();
    } else {
      ingContainer.classList.add("hidden");
    }

    document.getElementById("entity-modal").classList.add("active");
  },

  /**
   * Open edit modal
   */
  openEditModal(entityType, event) {
    event.stopPropagation();
    const row = event.target.closest("tr");
    const id = this.currentData.find((item, index) => {
      const rowIdx = Array.from(row.parentNode.children).indexOf(row) - 1;
      return index === rowIdx;
    })?.id;

    if (!id) return;

    const item = this.currentData.find((i) => i.id === id);
    if (!item) return;

    const config = this.config[this.currentEntity];
    document.getElementById("modal-title").textContent =
      `Редактировать: ${utils.escapeHtml(item.name)}`;
    document.getElementById("edit-id").value = item.id;
    utils.clearFormErrors("entity-form");

    // Render form fields with data
    const fieldsConfig = JSON.parse(JSON.stringify(config.fields));
    fieldsConfig.forEach((f) => {
      if (f.type === "select" && f.options) {
        f.options =
          this.currentEntity === "products"
            ? this.config.products.categories
            : this.config.dishes.categories;
      }
    });

    components.renderFormFields("modal-fields", fieldsConfig, item, config);

    // Handle flags - convert array to string
    if (item.flags && Array.isArray(item.flags)) {
      const flagsInput = document.getElementById("field-flags");
      if (flagsInput) {
        flagsInput.value = item.flags.filter((f) => f !== "None").join(",");
      }
    }

    // Show/hide ingredients section
    const ingContainer = document.getElementById("ingredients-container");
    if (config.hasIngredients && item.ingredients) {
      ingContainer.classList.remove("hidden");
      const ingList = document.getElementById("ingredients-list");
      ingList.innerHTML = "";
      this.selectedIngredients.clear();

      item.ingredients.forEach((ing) => {
        this.selectedIngredients.add(ing.productId);
        const product = this.products.find((p) => p.id === ing.productId);
        this.addIngredientRow(
          ing.productId,
          product?.name || "Unknown",
          ing.amountInGrams,
        );
      });

      this.updateIngredientSelect();
    } else {
      ingContainer.classList.add("hidden");
    }

    document.getElementById("entity-modal").classList.add("active");
  },

  /**
   * Open view modal
   */
  openViewModal(entityType, event) {
    event.stopPropagation();
    const row = event.target.closest("tr");
    const rowIdx = Array.from(row.parentNode.children).indexOf(row) - 1;
    const item = this.currentData[rowIdx];

    if (!item) return;

    const config = this.config[this.currentEntity];
    document.getElementById("view-modal-title").textContent = utils.escapeHtml(
      item.name,
    );

    // Fetch full details
    const apiMethod =
      this.currentEntity === "products"
        ? api.products.getById
        : api.dishes.getById;
    apiMethod(item.id)
      .then((fullItem) => {
        const content =
          this.currentEntity === "products"
            ? components.renderProductView(fullItem)
            : components.renderDishView(fullItem);
        document.getElementById("view-modal-content").innerHTML = content;
        document.getElementById("view-modal").classList.add("active");
      })
      .catch((error) => {
        this.showNotification(`Ошибка загрузки: ${error.message}`, "error");
      });
  },

  /**
   * Close modals
   */
  closeModal() {
    document.getElementById("entity-modal").classList.remove("active");
    this.selectedIngredients.clear();
  },

  closeViewModal() {
    document.getElementById("view-modal").classList.remove("active");
  },

  /**
   * Image viewer
   */
  openImageModal(images, index = 0) {
    if (!images || images.length === 0) return;

    this.currentImages = images;
    this.currentImageIndex = index;
    this.updateImageViewer();
    document.getElementById("image-modal").classList.add("active");
  },

  closeImageModal() {
    document.getElementById("image-modal").classList.remove("active");
  },

  navigateImage(direction) {
    this.currentImageIndex += direction;
    if (this.currentImageIndex < 0)
      this.currentImageIndex = this.currentImages.length - 1;
    if (this.currentImageIndex >= this.currentImages.length)
      this.currentImageIndex = 0;
    this.updateImageViewer();
  },

  updateImageViewer() {
    const img = document.getElementById("image-viewer-img");
    const counter = document.getElementById("image-counter");
    const prevBtn = document.getElementById("image-prev");
    const nextBtn = document.getElementById("image-next");

    img.src = this.currentImages[this.currentImageIndex];
    counter.textContent = `${this.currentImageIndex + 1} / ${this.currentImages.length}`;

    prevBtn.disabled = this.currentImages.length <= 1;
    nextBtn.disabled = this.currentImages.length <= 1;
  },

  /**
   * Add ingredient row
   */
  addIngredientRow(productId, productName, amount = "") {
    const container = document.getElementById("ingredients-list");
    const div = document.createElement("div");
    div.className = "ingredient-item";
    div.dataset.productId = productId;

    div.innerHTML = `
      <span class="product-name">${utils.escapeHtml(productName)}</span>
      <span class="product-amount">${amount ? `${utils.formatNumber(amount, 0)} г` : ""}</span>
      <input type="hidden" class="ing-product-id" value="${productId}">
      <input type="hidden" class="ing-amount" value="${amount}">
      <button type="button" class="btn btn-danger btn-sm" onclick="app.removeIngredient(this)">✕</button>
    `;

    container.appendChild(div);
  },

  /**
   * Add ingredient from selector
   */
  addIngredient() {
    const select = document.getElementById("ingredient-product-select");
    const amountInput = document.getElementById("ingredient-amount");

    const productId = parseInt(select.value);
    const amount = parseFloat(amountInput.value);

    if (!productId) {
      this.showNotification("Выберите продукт", "error");
      return;
    }

    if (!amount || amount <= 0) {
      this.showNotification("Введите корректное количество", "error");
      return;
    }

    const product = this.products.find((p) => p.id === productId);
    if (!product) return;

    this.selectedIngredients.add(productId);
    this.addIngredientRow(productId, product.name, amount);
    this.updateIngredientSelect();

    select.value = "";
    amountInput.value = "";
  },

  /**
   * Remove ingredient
   */
  removeIngredient(btn) {
    const row = btn.closest(".ingredient-item");
    const productId = parseInt(row.dataset.productId);
    this.selectedIngredients.delete(productId);
    row.remove();
    this.updateIngredientSelect();
  },

  /**
   * Handle form submit
   */
  async handleFormSubmit(event) {
    event.preventDefault();
    utils.clearFormErrors("entity-form");

    const id = document.getElementById("edit-id").value;
    const config = this.config[this.currentEntity];
    const payload = {};

    // Collect form data
    let hasErrors = false;
    config.fields.forEach((field) => {
      if (field.type === "array") {
        const container = document.getElementById(`container-${field.name}`);
        if (container) {
          const inputs = container.querySelectorAll(".array-item-input");
          const values = Array.from(inputs)
            .map((inp) => inp.value.trim())
            .filter((val) => val !== "");
          if (values.length > 0) {
            payload[field.name] = values;
          }
        }
        return;
      }

      const input = document.getElementById(`field-${field.name}`);
      if (input) {
        let val = input.value;
        if (val === "" && !field.required) return;

        // Validation
        const errors = utils.validateField(val, {
          required: field.required,
          minLength: field.minLength,
          min: field.min,
          max: field.max,
        });

        if (errors.length > 0) {
          utils.showFieldError(`field-${field.name}`, errors[0]);
          hasErrors = true;
          return;
        }

        if (field.type === "number") val = parseFloat(val);
        if (field.name === "flags" && typeof val === "string") {
          // Parse flags from comma-separated string
          val = val
            .split(",")
            .map((f) => f.trim())
            .filter((f) => f)
            .join(",");
        }
        payload[field.name] = val;
      }
    });

    if (hasErrors) {
      this.showNotification("Исправьте ошибки в форме", "error");
      return;
    }

    // Add ingredients for dishes
    if (config.hasIngredients) {
      const ingredients = [];
      document
        .querySelectorAll("#ingredients-list .ingredient-item")
        .forEach((row) => {
          const pId = parseInt(row.querySelector(".ing-product-id").value);
          const amt = parseFloat(row.querySelector(".ing-amount").value);
          if (pId && amt) {
            ingredients.push({ productId: pId, amountInGrams: amt });
          }
        });

      if (ingredients.length === 0 && !id) {
        this.showNotification("Добавьте хотя бы один ингредиент", "error");
        return;
      }

      payload.ingredients = ingredients;
    }

    try {
      const apiMethod = id
        ? this.currentEntity === "products"
          ? api.products.update
          : api.dishes.update
        : this.currentEntity === "products"
          ? api.products.create
          : api.dishes.create;

      const url = id ? `${config.endpoint}/${id}` : config.endpoint;
      const method = id ? "PATCH" : "POST";

      await api.request(url, {
        method: method,
        body: JSON.stringify(payload),
      });

      this.showNotification(
        id ? "Изменения сохранены" : "Успешно создано",
        "success",
      );
      this.closeModal();
      await this.loadData();
    } catch (error) {
      this.showNotification(`Ошибка: ${error.message}`, "error");
    }
  },

  /**
   * Delete item
   */
  async deleteItem(entityType, event) {
    event.stopPropagation();
    const row = event.target.closest("tr");
    const rowIdx = Array.from(row.parentNode.children).indexOf(row) - 1;
    const item = this.currentData[rowIdx];

    if (!item) return;

    if (
      !confirm(
        `Вы уверены, что хотите удалить "${utils.escapeHtml(item.name)}"?`,
      )
    ) {
      return;
    }

    try {
      const apiMethod =
        entityType === "products" ? api.products.delete : api.dishes.delete;
      await apiMethod(item.id);

      this.showNotification("Удалено", "success");
      await this.loadData();
    } catch (error) {
      this.showNotification(`Ошибка удаления: ${error.message}`, "error");
    }
  },

  /**
   * Reset filters
   */
  resetFilters() {
    document.getElementById("filter-form").reset();
    document
      .querySelectorAll(".flag-filter")
      .forEach((cb) => (cb.checked = false));
    this.loadData();
  },

  /**
   * Show notification
   */
  showNotification(message, type = "info") {
    const area = document.getElementById("notification-area");
    area.textContent = message;
    area.className = `notification-area ${type}`;

    setTimeout(() => {
      area.textContent = "";
      area.className = "notification-area";
    }, 3000);
  },
};

// Initialize on DOM ready
document.addEventListener("DOMContentLoaded", () => {
  app.init();
});
