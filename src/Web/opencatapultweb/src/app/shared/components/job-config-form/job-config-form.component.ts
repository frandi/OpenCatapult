import { Component, OnInit, Input } from '@angular/core';
import { CreateJobDefinitionDto } from '@app/core';

@Component({
  selector: 'app-job-config-form',
  templateUrl: './job-config-form.component.html',
  styleUrls: ['./job-config-form.component.css']
})
export class JobConfigFormComponent implements OnInit {
  @Input() jobDefinitions: CreateJobDefinitionDto[];

  constructor() { }

  ngOnInit() {
  }

}
