// http://fabiolandoni.ch/dictionary-in-typescript-based-on-arrays-of-generic-types/

export class Dictionary<T extends number | string, U> {
  private keyArray: T[] = [];
  private valueArray: U[] = [];

  private undefinedKeyErrorMessage: string =
    "Key is either undefined, null or an empty string.";

  private isEitherUndefinedNullOrStringEmpty(object: any): boolean {
    return (
      typeof object === "undefined" ||
      object === null ||
      object.toString() === ""
    );
  }

  private checkKeyAndPerformAction(
    action: { (key: T, value?: U): void | U | boolean },
    key: T,
    value?: U
  ): void | U | boolean {
    if (this.isEitherUndefinedNullOrStringEmpty(key)) {
      throw new Error(this.undefinedKeyErrorMessage);
    }

    return action(key, value);
  }

  public add(key: T, value: U): void {
    const addAction = (key: T, value: U): void => {
      if (this.containsKey(key)) {
        throw new Error(
          "An element with the same key already exists in the dictionary."
        );
      }

      this.keyArray.push(key);
      this.valueArray.push(value);
    };

    this.checkKeyAndPerformAction(addAction, key, value);
  }

  // mli
  public addOrUpdate(key: T, value: U) {
    if (this.containsKey(key)) {
      this.changeValueForKey(key, value);
    } else {
      this.add(key, value);
    }
  }

  public remove(key: T): boolean {
    const removeAction = (key: T): boolean => {
      if (!this.containsKey(key)) {
        return false;
      }

      const index = this.keyArray.indexOf(key);
      this.keyArray.splice(index, 1);
      this.valueArray.splice(index, 1);

      return true;
    };

    return this.checkKeyAndPerformAction(removeAction, key) as boolean;
  }

  // mli
  public removeByValue(value: U): boolean {
    const index = this.valueArray.indexOf(value);
    if (index === -1) {
      return false;
    }
    this.keyArray.splice(index, 1);
    this.valueArray.splice(index, 1);
    return true;
  }

  public getValue(key: T): U {
    const getValueAction = (key: T): U => {
      if (!this.containsKey(key)) {
        return null;
      }

      const index = this.keyArray.indexOf(key);
      return this.valueArray[index];
    };

    return this.checkKeyAndPerformAction(getValueAction, key) as U;
  }

  public containsKey(key: T): boolean {
    const containsKeyAction = (key: T): boolean => {
      if (this.keyArray.indexOf(key) === -1) {
        return false;
      }
      return true;
    };

    return this.checkKeyAndPerformAction(containsKeyAction, key) as boolean;
  }

  // mli
  public containsValue(value: U): boolean {
    const index = this.valueArray.indexOf(value);
    return index !== -1;
  }

  public changeValueForKey(key: T, newValue: U): void {
    const changeValueForKeyAction = (key: T, newValue: U): void => {
      if (!this.containsKey(key)) {
        throw new Error(
          "In the dictionary there is no element with the given key."
        );
      }

      const index = this.keyArray.indexOf(key);
      this.valueArray[index] = newValue;
    };

    this.checkKeyAndPerformAction(changeValueForKeyAction, key, newValue);
  }

  public keys(): T[] {
    return this.keyArray;
  }

  public values(): U[] {
    return this.valueArray;
  }

  public count(): number {
    return this.valueArray.length;
  }
}
