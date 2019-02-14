import { Component, OnInit, Input } from '@angular/core';
import { CreateJobTaskDefinitionDto } from '@app/core';

@Component({
  selector: 'app-task-config-list-form',
  templateUrl: './task-config-list-form.component.html',
  styleUrls: ['./task-config-list-form.component.css']
})
export class TaskConfigListFormComponent implements OnInit {
  @Input() tasks: CreateJobTaskDefinitionDto[];

  constructor() { }

  ngOnInit() {
  }

}
