interface Date {
  ToLongRussian(): string;
}

Date.prototype.ToLongRussian = () => {
  const mm = this.getMonth() + 1; // getMonth() is zero-based
  const dd = this.getDate();

  const hh = this.getHours();
  const min = this.getMinutes();
  const sec = this.getSeconds();

  return [
    (hh > 9 ? "" : "0") + hh,
    ":",
    (min > 9 ? "" : "0") + min,
    ":",
    (sec > 9 ? "" : "0") + sec,
    " ",
    (dd > 9 ? "" : "0") + dd,
    "/",
    (mm > 9 ? "" : "0") + mm,
    "/",
    this.getFullYear(),
  ].join("");
};
