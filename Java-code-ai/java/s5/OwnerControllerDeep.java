package java.s5;

import java.util.List;
import java.util.Map;
import javax.validation.Valid;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.samples.petclinic.visit.VisitRepository;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.web.bind.WebDataBinder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.ModelAndView;

@Controller
class OwnerControllerDeep {
    private static final String OWNER_FORM_VIEW = "owners/createOrUpdateOwnerForm";
    private static final int PAGE_SIZE = 5;

    private final OwnerRepository owners;
    private final VisitRepository visits;

    public OwnerControllerDeep(OwnerRepository owners, VisitRepository visits) {
        this.owners = owners;
        this.visits = visits;
    }

    @InitBinder
    public void setAllowedFields(WebDataBinder dataBinder) {
        dataBinder.setDisallowedFields("id");
    }

    // Owner Creation
    @GetMapping("/owners/new")
    public String showCreateForm(Map<String, Object> model) {
        model.put("owner", new Owner());
        return OWNER_FORM_VIEW;
    }

    @PostMapping("/owners/new")
    public String processCreateForm(@Valid Owner owner, BindingResult result) {
        if (result.hasErrors()) {
            return OWNER_FORM_VIEW;
        }
        this.owners.save(owner);
        return "redirect:/owners/" + owner.getId();
    }

    // Owner Search
    @GetMapping("/owners/find")
    public String showFindForm(Map<String, Object> model) {
        model.put("owner", new Owner());
        return "owners/findOwners";
    }

    @GetMapping("/owners")
    public String processFindForm(
            @RequestParam(defaultValue = "1") int page,
            Owner owner,
            BindingResult result,
            Model model) {

        String lastName = owner.getLastName() == null ? "" : owner.getLastName();
        Page<Owner> ownersPage = findPaginatedOwners(page, lastName);

        if (ownersPage.isEmpty()) {
            result.rejectValue("lastName", "notFound", "not found");
            return "owners/findOwners";
        }

        if (ownersPage.getTotalElements() == 1) {
            return "redirect:/owners/" + ownersPage.getContent().get(0).getId();
        }

        return buildPaginationModel(page, model, lastName, ownersPage);
    }

    // Owner Update
    @GetMapping("/owners/{ownerId}/edit")
    public String showUpdateForm(@PathVariable int ownerId, Model model) {
        model.addAttribute(this.owners.findById(ownerId));
        return OWNER_FORM_VIEW;
    }

    @PostMapping("/owners/{ownerId}/edit")
    public String processUpdateForm(
            @Valid Owner owner,
            BindingResult result,
            @PathVariable int ownerId) {
        
        if (result.hasErrors()) {
            return OWNER_FORM_VIEW;
        }
        
        owner.setId(ownerId);
        this.owners.save(owner);
        return "redirect:/owners/{ownerId}";
    }

    // Owner Details
    @GetMapping("/owners/{ownerId}")
    public ModelAndView showOwner(@PathVariable int ownerId) {
        ModelAndView mav = new ModelAndView("owners/ownerDetails");
        Owner owner = this.owners.findById(ownerId);
        loadPetVisits(owner);
        mav.addObject(owner);
        return mav;
    }

    // Private helper methods
    private Page<Owner> findPaginatedOwners(int page, String lastName) {
        Pageable pageable = PageRequest.of(page - 1, PAGE_SIZE);
        return owners.findByLastName(lastName, pageable);
    }

    private String buildPaginationModel(
            int page,
            Model model,
            String lastName,
            Page<Owner> paginated) {
        
        List<Owner> ownersList = paginated.getContent();
        model.addAttribute("listOwners", ownersList);
        model.addAttribute("currentPage", page);
        model.addAttribute("totalPages", paginated.getTotalPages());
        model.addAttribute("totalItems", paginated.getTotalElements());
        
        return "owners/ownersList";
    }

    private void loadPetVisits(Owner owner) {
        owner.getPets().forEach(pet -> 
            pet.setVisitsInternal(visits.findByPetId(pet.getId()))
        );
    }
}