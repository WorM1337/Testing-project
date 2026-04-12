/**
 * Utility functions for Book of Receipts
 */
const utils = {
  /**
   * Translates English enum values to Russian
   */
  translateEnum(value, type) {
    if (!value) return value;

    const translations = {
      // Product Categories
      ProductCategory: {
        None: "Не указано",
        Frozen: "Замороженный",
        Meat: "Мясной",
        Vegetables: "Овощи",
        Herbs: "Зелень",
        Spices: "Специи",
        Cereals: "Крупы",
        CannedFood: "Консервы",
        Liquid: "Жидкость",
        Sweets: "Сладости",
      },
      // Dish Categories
      DishCategory: {
        None: "Не указано",
        Dessert: "Десерт",
        Entree: "Первое",
        Side: "Второе",
        Drink: "Напиток",
        Salad: "Салат",
        Soup: "Суп",
        Snack: "Перекус",
      },
      // Cooking Requirements
      CookingRequirement: {
        ReadyToUse: "Готовый к употреблению",
        SemiFinished: "Полуфабрикат",
        RequiresCooking: "Требует приготовления",
      },
      // Flags
      ExtraFlag: {
        None: "",
        Vegan: "Веган",
        GlutenFree: "Без глютена",
        SugarFree: "Без сахара",
      },
      // Sort Options
      ProductSortOption: {
        Name: "Название",
        Calories: "Калорийность",
        Proteins: "Белки",
        Fats: "Жиры",
        Carbs: "Углеводы",
        CreatedAt: "Дата создания",
      },
      DishSortOption: {
        Name: "Название",
        Calories: "Калорийность порции",
        Proteins: "Белки",
        Fats: "Жиры",
        Carbs: "Углеводы",
        Category: "Категория",
      },
    };

    const typeTranslations = translations[type];
    if (typeTranslations) {
      return typeTranslations[value] || value;
    }

    return value;
  },

  /**
   * Formats flags array to readable Russian string
   */
  formatFlags(flags) {
    if (!flags || flags === "None") return [];

    const flagList = typeof flags === "string" ? flags.split(", ") : flags;
    return flagList
      .filter((f) => f && f !== "None")
      .map((f) => this.translateEnum(f.trim(), "ExtraFlag"));
  },

  /**
   * Formats number with fixed decimals
   */
  formatNumber(num, decimals = 1) {
    if (num === null || num === undefined || num === "") return "—";
    return parseFloat(num).toFixed(decimals);
  },

  /**
   * Formats date to Russian locale
   */
  formatDate(dateString) {
    if (!dateString) return "—";
    return new Date(dateString).toLocaleString("ru-RU", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  },

  /**
   * Validates form field
   */
  validateField(value, rules) {
    const errors = [];

    if (
      rules.required &&
      (!value || (typeof value === "string" && !value.trim()))
    ) {
      errors.push("Это поле обязательно для заполнения");
    }

    if (
      rules.minLength &&
      typeof value === "string" &&
      value.length < rules.minLength
    ) {
      errors.push(`Минимальная длина — ${rules.minLength} символов`);
    }

    if (
      rules.maxLength &&
      typeof value === "string" &&
      value.length > rules.maxLength
    ) {
      errors.push(`Максимальная длина — ${rules.maxLength} символов`);
    }

    if (
      rules.min !== undefined &&
      typeof value === "number" &&
      value < rules.min
    ) {
      errors.push(`Минимальное значение — ${rules.min}`);
    }

    if (
      rules.max !== undefined &&
      typeof value === "number" &&
      value > rules.max
    ) {
      errors.push(`Максимальное значение — ${rules.max}`);
    }

    if (
      rules.pattern &&
      typeof value === "string" &&
      !rules.pattern.test(value)
    ) {
      errors.push(rules.patternMessage || "Неверный формат");
    }

    return errors;
  },

  /**
   * Shows field error
   */
  showFieldError(fieldId, message) {
    const formGroup = document.getElementById(fieldId)?.closest(".form-group");
    if (!formGroup) return;

    formGroup.classList.add("error");

    let errorEl = formGroup.querySelector(".error-message");
    if (!errorEl) {
      errorEl = document.createElement("div");
      errorEl.className = "error-message";
      formGroup.appendChild(errorEl);
    }

    errorEl.textContent = message;
  },

  /**
   * Clears field error
   */
  clearFieldError(fieldId) {
    const formGroup = document.getElementById(fieldId)?.closest(".form-group");
    if (!formGroup) return;

    formGroup.classList.remove("error");

    const errorEl = formGroup.querySelector(".error-message");
    if (errorEl) {
      errorEl.remove();
    }
  },

  /**
   * Clears all form errors
   */
  clearFormErrors(formId) {
    const form = document.getElementById(formId);
    if (!form) return;

    form.querySelectorAll(".form-group.error").forEach((group) => {
      group.classList.remove("error");
    });

    form.querySelectorAll(".error-message").forEach((el) => {
      el.remove();
    });
  },

  /**
   * Escapes HTML to prevent XSS
   */
  escapeHtml(text) {
    if (typeof text !== "string") return text;
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
  },

  /**
   * Debounce function
   */
  debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  },

  /**
   * Generates unique ID
   */
  generateId() {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  },
};
