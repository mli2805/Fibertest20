import { Component, OnInit, ViewChild } from "@angular/core";
import { MatMenuTrigger } from "@angular/material";

@Component({
  selector: "ft-network-events",
  templateUrl: "./ft-network-events.component.html",
  styleUrls: ["./ft-network-events.component.css"],
})
export class FtNetworkEventsComponent implements OnInit {
  displayedColumns: string[] = ["name", "surname"];

  items = [
    { id: 1, name: "Item 1", surname: "Surname 1" },
    { id: 2, name: "Item 2", surname: "Surname 2" },
    { id: 3, name: "Item 3", surname: "Surname 3" },
  ];

  @ViewChild(MatMenuTrigger, null)
  contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: "0px", y: "0px" };

  constructor() {}
  ngOnInit() {}

  onContextMenu(event: MouseEvent, item: Item) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + "px";
    this.contextMenuPosition.y = event.clientY + "px";
    this.contextMenu.menuData = { item };
    this.contextMenu.menu.focusFirstItem("mouse");
    this.contextMenu.openMenu();
  }

  onContextMenuAction1(item: Item) {
    alert(`Click on Action 1 for ${item.name}`);
  }

  onContextMenuAction2(item: Item) {
    alert(`Click on Action 2 for ${item.name}`);
  }
}

export interface Item {
  id: number;
  name: string;
  surname: string;
}
