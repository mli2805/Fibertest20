import { Component, OnInit } from "@angular/core";
import { Frequency } from "src/app/models/enums/frequency";
import { FrequencyPipe } from "src/app/pipes/frequency.pipe";

@Component({
  selector: "ft-about",
  templateUrl: "./ft-about.component.html",
  styleUrls: ["./ft-about.component.css"]
})
export class FtAboutComponent implements OnInit {
  selectedItem;
  itemsSource;

  constructor(private frequencyPipe: FrequencyPipe) {}

  ngOnInit() {
    const frs = Object.keys(Frequency)
      .filter(e => !isNaN(+e))
      .map(e => {
        return { index: +e, name: this.frequencyPipe.transform(+e) };
      });
    this.itemsSource = frs;
    console.log(this.itemsSource);
    this.selectedItem = frs[0].index;
    console.log(this.selectedItem);
  }
}
