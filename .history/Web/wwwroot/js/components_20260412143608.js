/**
 * UI Components for Book of Receipts
 */
const components = {
  /**
   * Renders form fields based on configuration
   */
  renderFormFields(containerId, fields, data = {}, config = {}) {
    const container = document.getElementById(containerId);
    if (!container) return;

    container.innerHTML = "";

    fields.forEach((field) => {
      const formGroup = document.createElement("div");
      formGroup.className = "form-group";
      if (field.type === "array" || field.type === "textarea") {
        formGroup.classList.add("full-width");
      }

      const label = document.createElement("label");
      label.htmlFor = `field-${field.name}`;
      label.textContent = field.label + (field.required ? " *" : "");
      formGroup.appendChild(label);

      let input;

      switch (field.type) {
        case "select":
          input = this.createSelect(field, data[field.name], config);
          break;
        case "textarea":
          input = this.createTextarea(field, data[field.name]);
          break;
        case "array":
          input = this.createArrayField(field, data[field.name]);
          break;
        case "number":
          input = this.createNumberInput(field, data[field.name]);
          break;
        default:
          input = this.createTextInput(field, data[field.name]);
      }

      formGroup.appendChild(input);
      container.appendChild(formGroup);
    });
  },

  createSelect(field, value, config) {
    const select = document.createElement("select");
    select.id = `field-${field.name}`;
    select.name = field.name;
    select.className = "form-control";
    if (field.required) select.required = true;

    const defaultOption = document.createElement("option");
    defaultOption.value = "";
    defaultOption.textContent = `— Выберите ${field.label.toLowerCase()} —`;
    select.appendChild(defaultOption);

    const options = field.options || config.categories || [];
    options.forEach((opt) => {
      const option = document.createElement("option");
      // Handle both object format {val, label} and string format
      if (typeof opt === "object" && opt.val) {
        option.value = opt.val;
        option.textContent = opt.label;
      } else {
        option.value = opt;
        option.textContent = utils.translateEnum(
          opt,
          field.categoryType || "ProductCategory",
        );
      }
      if (value === opt.val || value === opt) option.selected = true;
      select.appendChild(option);
    });

    return select;
  },

  createTextarea(field, value) {
    const textarea = document.createElement("textarea");
    textarea.id = `field-${field.name}`;
    textarea.name = field.name;
    textarea.className = "form-control";
    textarea.rows = field.rows || 3;
    if (field.required) textarea.required = true;
    if (value) textarea.value = value;
    return textarea;
  },

  createTextInput(field, value) {
    const input = document.createElement("input");
    input.type = field.type || "text";
    input.id = `field-${field.name}`;
    input.name = field.name;
    input.className = "form-control";
    if (field.required) input.required = true;
    if (field.placeholder) input.placeholder = field.placeholder;
    if (value) input.value = value;
    return input;
  },

  createNumberInput(field, value) {
    const input = document.createElement("input");
    input.type = "number";
    input.id = `field-${field.name}`;
    input.name = field.name;
    input.className = "form-control";
    if (field.required) input.required = true;
    if (field.step) input.step = field.step;
    if (field.min !== undefined) input.min = field.min;
    if (field.max !== undefined) input.max = field.max;
    if (field.placeholder) input.placeholder = field.placeholder;
    if (value !== null && value !== undefined) input.value = value;
    return input;
  },

  createArrayField(field, values = []) {
    const container = document.createElement("div");
    container.id = `container-${field.name}`;
    container.className = "array-field";

    const itemsContainer = document.createElement("div");
    itemsContainer.className = "photo-preview";
    container.appendChild(itemsContainer);

    const addButton = document.createElement("button");
    addButton.type = "button";
    addButton.className = "btn btn-secondary btn-sm mt-4";
    addButton.textContent = `+ Добавить ${field.itemLabel || "значение"}`;
    addButton.onclick = () => {
      this.addArrayItem(field.name, field.itemType || "text", itemsContainer);
    };
    container.appendChild(addButton);

    // Fill existing values
    if (values && values.length > 0) {
      values.forEach((val) => {
        this.addArrayItem(
          field.name,
          field.itemType || "text",
          itemsContainer,
          val,
        );
      });
    }

    return container;
  },

  addArrayItem(fieldName, itemType, container, value = "") {
    const wrapper = document.createElement("div");
    wrapper.className = "photo-thumbnail";

    if (itemType === "url") {
      const img = document.createElement("img");
      img.src =
        value ||
        "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80' viewBox='0 0 24 24' fill='none' stroke='%23ccc' stroke-width='2'%3E%3Crect x='3' y='3' width='18' height='18' rx='2'/%3E%3Ccircle cx='8.5' cy='8.5' r='1.5'/%3E%3Cpolyline points='21 15 16 10 5 21'/%3E%3C/svg%3E";
      img.alt = "Preview";
      img.onclick = () => app.openImageModal([value], 0);
      wrapper.appendChild(img);
    }

    const input = document.createElement("input");
    input.type = itemType === "url" ? "hidden" : itemType;
    input.className = "array-item-input";
    input.dataset.field = fieldName;
    input.value = value || "";
    if (itemType !== "url") {
      input.style.width = "100%";
      input.style.marginTop = "8px";
      wrapper.appendChild(input);
    } else {
      wrapper.querySelector("img").dataset.url = value || "";
    }

    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.className = "remove-photo";
    removeBtn.innerHTML = "×";
    removeBtn.onclick = () => wrapper.remove();
    wrapper.appendChild(removeBtn);

    container.appendChild(wrapper);

    if (itemType !== "url") {
      input.focus();
    }
  },

  /**
   * Renders table headers
   */
  renderTableHeaders(rowId, columns) {
    const row = document.getElementById(rowId);
    if (!row) return;

    row.innerHTML =
      columns.map((col) => `<th>${col.label}</th>`).join("") +
      '<th style="width: 180px;">Действия</th>';
  },

  /**
   * Renders table body with data
   */
  renderTableBody(rowId, data, columns, actions) {
    const tbody = document.getElementById(rowId);
    if (!tbody) return;

    if (!data || data.length === 0) {
      tbody.innerHTML = "";
      return;
    }

    tbody.innerHTML = data
      .map((item) => {
        const cells = columns
          .map((col) => {
            if (col.type === "flags") {
              const flags = utils.formatFlags(item.flags);
              return `<td>${flags.map((f) => `<span class="badge badge-flag">${f}</span>`).join("")}</td>`;
            }
            if (col.type === "category") {
              const category = utils.translateEnum(
                item[col.key],
                col.categoryType,
              );
              return `<td><span class="badge badge-category">${category}</span></td>`;
            }
            if (col.type === "cooking") {
              const cooking = utils.translateEnum(
                item[col.key],
                "CookingRequirement",
              );
              return `<td><span class="badge badge-cooking">${cooking}</span></td>`;
            }
            if (
              col.type === "photos" &&
              item.photos &&
              item.photos.length > 0
            ) {
              return `<td><span class="badge" style="cursor: pointer;" onclick="app.openImageModal(${JSON.stringify(item.photos)}, 0)">📷 ${item.photos.length}</span></td>`;
            }
            const value =
              item[col.key] !== null && item[col.key] !== undefined
                ? item[col.key]
                : "—";
            return `<td>${utils.escapeHtml(value)}</td>`;
          })
          .join("");

        const actionButtons = actions
          .map((action) => {
            const btnClass =
              action.type === "danger"
                ? "btn-danger"
                : action.type === "success"
                  ? "btn-success"
                  : "btn-secondary";
            return `<button class="btn ${btnClass} btn-sm" onclick="${action.onClick}">${action.label}</button>`;
          })
          .join("");

        return `<tr>${cells}<td style="display: flex; gap: 4px; flex-wrap: wrap;">${actionButtons}</td></tr>`;
      })
      .join("");
  },

  /**
   * Renders view modal content for product
   */
  renderProductView(product) {
    const flags = utils.formatFlags(product.flags);
    const photos = product.photos || [];

    return `
      <div class="view-detail">
        <div class="view-detail-label">Название</div>
        <div class="view-detail-value" style="font-size: 1.25rem; font-weight: 600;">${utils.escapeHtml(product.name)}</div>
      </div>
      
      ${
        photos.length > 0
          ? `
        <div class="view-detail">
          <div class="view-detail-label">Фотографии</div>
          <div class="photo-preview">
            ${photos
              .map(
                (url, i) => `
              <div class="photo-thumbnail" onclick="app.openImageModal(${JSON.stringify(photos)}, ${i})">
                <img src="${url}" alt="Photo ${i + 1}">
              </div>
            `,
              )
              .join("")}
          </div>
        </div>
      `
          : ""
      }
      
      <div class="view-nutrition">
        <div class="nutrition-item">
          <div class="nutrition-value">${utils.formatNumber(product.caloriesPer100g)}</div>
          <div class="nutrition-label">Ккал</div>
        </div>
        <div class="nutrition-item">
          <div class="nutrition-value">${utils.formatNumber(product.proteinsPer100g)}</div>
          <div class="nutrition-label">Белки, г</div>
        </div>
        <div class="nutrition-item">
          <div class="nutrition-value">${utils.formatNumber(product.fatsPer100g)}</div>
          <div class="nutrition-label">Жиры, г</div>
        </div>
        <div class="nutrition-item">
          <div class="nutrition-value">${utils.formatNumber(product.carbsPer100g)}</div>
          <div class="nutrition-label">Углеводы, г</div>
        </div>
      </div>
      
      <div class="view-detail">
        <div class="view-detail-label">Категория</div>
        <div class="view-detail-value"><span class="badge badge-category">${utils.translateEnum(product.category, "ProductCategory")}</span></div>
      </div>
      
      <div class="view-detail">
        <div class="view-detail-label">Требования к готовке</div>
        <div class="view-detail-value"><span class="badge badge-cooking">${utils.translateEnum(product.cookingRequirement, "CookingRequirement")}</span></div>
      </div>
      
      ${
        flags.length > 0
          ? `
        <div class="view-detail">
          <div class="view-detail-label">Флаги</div>
          <div class="view-detail-value">${flags.map((f) => `<span class="badge badge-flag">${f}</span>`).join("")}</div>
        </div>
      `
          : ""
      }
      
      ${
        product.composition
          ? `
        <div class="view-detail">
          <div class="view-detail-label">Состав</div>
          <div class="view-detail-value">${utils.escapeHtml(product.composition)}</div>
        </div>
      `
          : ""
      }
      
      <div class="view-detail">
        <div class="view-detail-label">Создано</div>
        <div class="view-detail-value">${utils.formatDate(product.createdAt)}</div>
      </div>
      
      ${
        product.updatedAt
          ? `
        <div class="view-detail">
          <div class="view-detail-label">Обновлено</div>
          <div class="view-detail-value">${utils.formatDate(product.updatedAt)}</div>
        </div>
      `
          : ""
      }
    `;
  },

  /**
   * Renders view modal content for dish
   */
  renderDishView(dish) {
    const flags = utils.formatFlags(dish.flags);
    const photos = dish.photos || [];

    return `
      <div class="view-detail">
        <div class="view-detail-label">Название</div>
        <div class="view-detail-value" style="font-size: 1.25rem; font-weight: 600;">${utils.escapeHtml(dish.name)}</div>
      </div>
      
      ${
        photos.length > 0
          ? `
        <div class="view-detail">
          <div class="view-detail-label">Фотографии</div>
          <div class="photo-preview">
            ${photos
              .map(
                (url, i) => `
              <div class="photo-thumbnail" onclick="app.openImageModal(${JSON.stringify(photos)}, ${i})">
                <img src="${url}" alt="Photo ${i + 1}">
              </div>
            `,
              )
              .join("")}
          </div>
        </div>
      `
          : ""
      }
      
      <div class="view-nutrition">
        <div class="nutrition-item">
          <div class="nutrition-value">${utils.formatNumber(dish.caloriesPerServing)}</div>
          <div class="nutrition-label">Ккал</div>
        </div>
        <div class="nutrition-item">
          <div class="nutrition-value">${utils.formatNumber(dish.proteinsPerServing)}</div>
          <div class="nutrition-label">Белки, г</div>
        </div>
        <div class="nutrition-item">
          <div class="nutrition-value">${utils.formatNumber(dish.fatsPerServing)}</div>
          <div class="nutrition-label">Жиры, г</div>
        </div>
        <div class="nutrition-item">
          <div class="nutrition-value">${utils.formatNumber(dish.carbsPerServing)}</div>
          <div class="nutrition-label">Углеводы, г</div>
        </div>
      </div>
      
      <div class="view-detail">
        <div class="view-detail-label">Размер порции</div>
        <div class="view-detail-value">${utils.formatNumber(dish.servingSize, 0)} г</div>
      </div>
      
      <div class="view-detail">
        <div class="view-detail-label">Категория</div>
        <div class="view-detail-value"><span class="badge badge-category">${utils.translateEnum(dish.category, "DishCategory")}</span></div>
      </div>
      
      ${
        flags.length > 0
          ? `
        <div class="view-detail">
          <div class="view-detail-label">Флаги</div>
          <div class="view-detail-value">${flags.map((f) => `<span class="badge badge-flag">${f}</span>`).join("")}</div>
        </div>
      `
          : ""
      }
      
      ${
        dish.ingredients && dish.ingredients.length > 0
          ? `
        <div class="view-detail">
          <div class="view-detail-label">Состав</div>
          <div class="view-detail-value">
            <ul style="margin: 8px 0; padding-left: 20px;">
              ${dish.ingredients
                .map(
                  (ing) => `
                <li>${utils.escapeHtml(ing.productName)} — ${utils.formatNumber(ing.amountInGrams, 0)} г</li>
              `,
                )
                .join("")}
            </ul>
          </div>
        </div>
      `
          : ""
      }
      
      <div class="view-detail">
        <div class="view-detail-label">Создано</div>
        <div class="view-detail-value">${utils.formatDate(dish.createdAt)}</div>
      </div>
      
      ${
        dish.updatedAt
          ? `
        <div class="view-detail">
          <div class="view-detail-label">Обновлено</div>
          <div class="view-detail-value">${utils.formatDate(dish.updatedAt)}</div>
        </div>
      `
          : ""
      }
    `;
  },
};
